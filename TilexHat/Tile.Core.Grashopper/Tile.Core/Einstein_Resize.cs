using System.Collections.Generic;
using System;
using System.Linq;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using Tile.Core.Util;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Tile.Core
{
    public enum Label
    {
        H = 0,
        H1 = 1,
        T = 2,
        P = 3,
        F = 4
    }
    public struct TilePatterns
    {
        public bool HasFrame;
        public Label label;
        public List<GeometryBase> Patterns;
        public bool Frame;
        public List<ObjectAttributes> PatternAtts;
        public bool ColourFromObject;
        public List<System.Guid> Guids;
    }
    /// <summary>
    /// This class provide an advance version for resize and customise hat, and
    /// also turns it into a block; therefore it can facilitate arranging into different
    /// tessellation. 
    /// </summary>
    public class Einstein_Resize : Einstein, IPermutation
    {
        private double Hatsize = 1;
        public int[] BlocksId = new int[5];
        private Transform Translation = new Transform();
        private HatGroup<int> _HatID;
        public List<GeometryBase> HPatterns = new List<GeometryBase>();
        private TilePatterns[] PatternsManager = new TilePatterns[5];
        /// <summary>
        /// Useless
        /// </summary>
        /// <returns></returns>
        public List<Curve> PreviewShape()
        {
            if (SetTile.Hat_Transform.Count <= 0) return new List<Curve>();
            object LockObj = new object();
            ConcurrentBag<Curve> HatCrvs = new ConcurrentBag<Curve>();
            ConcurrentBag<int> Seq = new ConcurrentBag<int>();
            var TS = this.SetTile.Hat_Transform;
            var Scale = Transform.Scale(Point3d.Origin, Hatsize);
            Parallel.For(0, TS.Count, i =>
            {
                var HatShape = new Einstein.HatTile("Outline").PreviewShape;
                var Final = Translation * Scale * TS[i];
                Seq.Add(i);
                HatShape.Transform(Final);
                HatCrvs.Add(HatShape);
            });
            List<Curve> sortedCurve = new List<Curve>();
            lock (LockObj)
            {
                var indexes = Seq.ToList();
                indexes.Sort();
                sortedCurve = indexes.Select(i => HatCrvs.ElementAt(i)).ToList();
            }
            return sortedCurve;
        }
        public Einstein SetTile { private get; set; } = new Einstein();
        public Einstein_Resize(double size, Point3d StartPt) : base()
        {
            if (size < 0) this.Hatsize = 1;
            else
                this.Hatsize = size;
            this.Translation = Transform.Translation(new Vector3d(StartPt.X, StartPt.Y, StartPt.Z));
            Label[] LabelTags = { Label.H, Label.H1, Label.T, Label.P, Label.F };
            for (int i = 0; i < this.PatternsManager.Length; i++)
            {
                var Patterns = new TilePatterns();
                Patterns.label = LabelTags[i];
                Patterns.Patterns = new List<GeometryBase>();
                Patterns.PatternAtts = new List<ObjectAttributes>();
                Patterns.Guids = new List<System.Guid>();
                Patterns.ColourFromObject = false;
                this.PatternsManager[i] = Patterns;
            }
        }
        public void NewSetPatterns(List<AddPatternOption> Options)
        {
            Util.Util.NewSetPatterns(Options, ref this.PatternsManager);
        }
        public void NewSetFrame()
        {
            Util.Util.NewSetFrame(ref PatternsManager);
        }
        public bool SetNewBlock(List<string> Name = null, bool Blockoverride = true)
        {
            _HatID = new HatGroup<int>(Util.Util.SetNewBlock(ref this.PatternsManager, Name, Blockoverride));
            return true;
        }
        public bool PlaceBlock(Einstein MonoTile)
        {
            if (this.Hatsize < 0 || MonoTile.Hat_Labels.Count != MonoTile.Hat_Transform.Count ||
                MonoTile.Hat_Labels.Count < 0)
                return false;
            string[] LayerName = { "Hat_H", "Hat_H1", "Hat_T", "Hat_P", "Hat_F" };
            var Doc = RhinoDoc.ActiveDoc;

            if (_HatID.Hat_F_ID < 0)
                throw new Exception("Objects hasn't been defined as blocks");

            var labels = MonoTile.Hat_Labels;
            var Transforms = MonoTile.Hat_Transform;
            var Scale = Transform.Scale(Point3d.Origin, Hatsize);
            for (int i = 0; i < Transforms.Count; i++)
            {
                var Final = Translation * Scale * Transforms[i];
                ObjectAttributes Att = new ObjectAttributes();
                switch (labels[i])
                {
                    case "H":
                        Att.LayerIndex = Doc.Layers.FindName(LayerName[0]).Index;
                        Doc.Objects.AddInstanceObject(_HatID[0], Final, Att);
                        break;
                    case "H1":
                        Att.LayerIndex = Doc.Layers.FindName(LayerName[1]).Index;
                        Doc.Objects.AddInstanceObject(_HatID[1], Final, Att);
                        break;
                    case "T":
                        Att.LayerIndex = Doc.Layers.FindName(LayerName[2]).Index;
                        Doc.Objects.AddInstanceObject(_HatID[2], Final, Att);
                        break;
                    case "P":
                        Att.LayerIndex = Doc.Layers.FindName(LayerName[3]).Index;
                        Doc.Objects.AddInstanceObject(_HatID[3], Final, Att);
                        break;
                    case "F":
                        Att.LayerIndex = Doc.Layers.FindName(LayerName[4]).Index;
                        Doc.Objects.AddInstanceObject(_HatID[4], Final, Att);
                        break;
                    default:
                        return false;
                }
            }
            return true;
        }
    }

}
