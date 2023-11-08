using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rhino.DocObjects;
using Rhino;
using Rhino.Geometry;
using Tile.Core.Util;
using Tile.Core.Hull;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;

namespace Tile.Core
{
    public class ExpensionData:ICloneable
    {
        public int Level;
        public Einstein.Type Type { get; }
        public List<Transform> HatTransform { get; private set; }
        public List<string> HatLabel { get; private set; }
        public List<Point3d> HatPt { get; private set; }
        public Concave_Hull_Object ConcaveHull;
        public bool Result { get; private set; }
        public ExpensionData(Einstein.Type Type)
        {
            this.Type = Type;
            HatPt = new List<Point3d>();
            HatTransform = new List<Transform>();
            
            Result = false;
            this.Level = 0;
        }
        public void SetResult(bool Result)
        {
            this.Result = Result;
        }
        public void SetTransform(List<Transform> HatTransform)
        {
            this.HatTransform = HatTransform;
        }
        public void SetLabels(List<string> Labels)
        {
            this.HatLabel = Labels;
        }
        public void SetTransformPoint(List<Point3d> Pts)
        {
            this.HatPt = Pts;
        }
        public void SetConcaveHull(Concave_Hull_Object hull_Object)
        {
            this.ConcaveHull = hull_Object;
        }
        public object Clone()
        {
            var Copy = new ExpensionData(this.Type);
            Copy.HatTransform = new List<Transform>(this.HatTransform);
            Copy.HatPt = new List<Point3d>(this.HatPt);
            Copy.ConcaveHull = this.ConcaveHull;
            Copy.Result = this.Result;
            Copy.Level = this.Level;
            return Copy;
        }
    }
    public class Einstein_Forming : Einstein, IPermutation
    {
        private double Hatsize = 1;
        public int[] BlocksId = new int[5];
        public double Alpha = 1.1;
        private Transform Translation = new Transform();
        private TilePatterns[] PatternsManager = new TilePatterns[5]; //H H1 T P F
        private Einstein _InternalEinstein = new Einstein();
        public ExpensionData FittingArrangement { get; private set; } = null;
        public bool HasFit { get; private set; } = false;
        public int FinalLevel { get; private set; } = 0;
        private List<Curve> Regions = new List<Curve>();
        public Convex_Hull_Object ConvexHull { get; private set; }
        public Point3d CentrePt { get; private set; } = new Point3d();
        public Einstein_Forming(IEnumerable<Curve> Curves, Point3d StartPt, 
            double Hatsize = 1, double Tor = 3):base()
        {
            this.Hatsize = Hatsize < 0 ? 1 : Hatsize;
            this.Regions = Curves.ToList();
            var HullObj = Convex_Hull.ConvexHull2D(Curves, Tor);
            this.ConvexHull = HullObj;


            if (StartPt == Point3d.Unset)
            {
                CentrePt = ConvexHull.GetPolyCurve.ToPolyline().CenterPoint();
            }
            else
                CentrePt = StartPt;
            
            this.Translation = Transform.Translation(new Vector3d(CentrePt));

            this.Hatsize = Hatsize;

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
        public ExpensionData EinIteration(int Count = 0)
        {
            if (Count == 0)
            {
                Count++;
                this._InternalEinstein = new Einstein();
                return EinIteration(Count);
            }
            else if (Count == 6)
                return null;
            else
            {
                this._InternalEinstein.NextIteration();

                ExpensionData ExpData = new ExpensionData(Type.T);

                if (TestForm())
                    ExpData = TestEinExpension(Type.H);
                else
                    ExpData = TestEinExpension(Type.P);

                if (ExpData.Result)
                {
                    Count++;
                    return EinIteration(Count);
                }
                else
                {
                    FinalLevel = Count;
                    return ExpData;
                }
            }
        }
        private bool TestForm()
        {
            var BBox = new BoundingBox(this.ConvexHull.GetPoints);
            var Box = new Box(BBox);
            var X_inter = Box.X.Length;
            var Y_inter = Box.Y.Length;

            double Ratio = X_inter / Y_inter;
            if (Ratio < 1.570)
                return true;

            return false; 
        }
        private ExpensionData TestEinExpension(Einstein.Type type)
        {
            var CopyEin = this._InternalEinstein.Clone() as Einstein;
            var ExpData = new ExpensionData(type);
            var Tile = CopyEin.Draw(type);
            List<Point3d> PtSet = this.PermuteHatPts(new List<Transform>(CopyEin.Hat_Transform));
            var TempPtSet = CentralisePts(PtSet);
            var Hull = Concave_Hull.ConcaveHull2D(TempPtSet, 1.1);
            List<Point3d> OutPts = Hull.GetPoints;
            bool c = IsAllInsideConcaveHull(OutPts);
            ExpData.SetResult(c);
            ExpData.Level = CopyEin.Level;
            ExpData.SetTransform(CopyEin.Hat_Transform);
            ExpData.SetTransformPoint(TempPtSet);
            ExpData.SetConcaveHull(Hull);
            ExpData.SetLabels(CopyEin.Hat_Labels);
            return ExpData;
        }
        private bool IsAllInsideConcaveHull(List<Point3d> ConcavePts)
        {
            bool c = false;
            foreach(var Pt in ConcavePts)
            {
                c = c || ConvexHull.GetPolyCurve.Contains(Pt, Plane.WorldXY, 0.1) == PointContainment.Inside;
            }
            return c;
        }
        Vector3d Adjust = new Vector3d(0, 0, 0);
        private List<Point3d> CentralisePts(List<Point3d> Pts)
        {
            var TempConcaveHull = Concave_Hull.ConcaveHull2D(Pts, 1.1).GetPolyCurve;

            var CentralPoint = (TempConcaveHull as PolylineCurve).ToPolyline().CenterPoint();
            
            Adjust = CentralPoint - this.CentrePt;
            Adjust.Reverse();
            List<Point3d> NewPts = new List<Point3d>();
            foreach (var Pt in Pts)
            {
                var NewPt = Pt + Adjust;
                NewPts.Add(NewPt);
            }

            return NewPts;
        }
        private List<Point3d> PermuteHatPts(List<Transform> HatTS)
        {
            ConcurrentBag<Point3d> PtsSet = new ConcurrentBag<Point3d>();
            ConcurrentBag<int> Seq = new ConcurrentBag<int>();
            object LockObject = new object();
            var Scale = Transform.Scale(Point3d.Origin, Hatsize);
            Parallel.For(0, HatTS.Count, i => 
            {
                var Final = Translation * Scale * HatTS[i];
                var Pt = Point3d.Origin;
                Seq.Add(i);
                Pt.Transform(Final);
                PtsSet.Add(Pt);
            });

            var PtList = new List<Point3d>();

            lock (LockObject)
            {
                var indexes = Seq.ToList();
                indexes.Sort();
                PtList = Seq.Select(x => PtsSet.ElementAt(x)).ToList();
            }

            return PtList;
        }
        public void NewSetPatterns(List<AddPatternOption> Options)
        {
            Util.Util.NewSetPatterns(Options, ref this.PatternsManager);
        }
        public void NewSetFrame()
        {
            Util.Util.NewSetFrame(ref PatternsManager);
        }
        private HatGroup<int> _HatID;
        public bool SetNewBlock(List<string> Name, bool Blockoverride = true)
        {
            _HatID = new HatGroup<int>(Util.Util.SetNewBlock(ref this.PatternsManager, Name, Blockoverride));
            return false;
        }
        public void StartFitting()
        {
            this.FittingArrangement = TrimOuterTransform(EinIteration());
            HasFit = true;
        }
        public List<Curve> PreviewShape() 
        {
            var Scale = Transform.Scale(Point3d.Origin, Hatsize);
            var AdjustTS = Transform.Translation(Adjust);
            object LockObj = new object();
            ConcurrentBag<Curve> HatCrvs = new ConcurrentBag<Curve>();
            ConcurrentBag<int> Seq = new ConcurrentBag<int>();
            var TS = this.FittingArrangement.HatTransform;
            Parallel.For(0, TS.Count, i => 
            {
                var HatShape = new Einstein.HatTile("Outline").PreviewShape;
                var Final = Translation * AdjustTS * Scale * TS[i];
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
        public bool PlaceBlock(Einstein MonoTile = null)
        {
            if (this.FittingArrangement is null) return false;
            var Doc = RhinoDoc.ActiveDoc;
            var labels = this.FittingArrangement.HatLabel;
            var Transforms = this.FittingArrangement.HatTransform;
            var Scale = Transform.Scale(Point3d.Origin, Hatsize);
            var AdjustTS = Transform.Translation(Adjust);
            string[] LayerName = { "Hat_H", "Hat_H1", "Hat_T", "Hat_P", "Hat_F" };

            if (_HatID.Hat_F_ID < 0)
                throw new Exception("Objects hasn't been defined as blocks");

            for (int i = 0; i < Transforms.Count; i++)
            {
                var Final = Translation * AdjustTS  * Scale * Transforms[i];
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
        private ExpensionData TrimOuterTransform(ExpensionData ExpData)
        {
            if (ExpData.HatPt.Count == 0) return null;
            

            ConcurrentBag<int> Seq = new ConcurrentBag<int>();
            ConcurrentBag<Point3d> Pts = new ConcurrentBag<Point3d>();
            ConcurrentBag<Transform> TS = new ConcurrentBag<Transform>();
            ConcurrentBag<string> HatLabels = new ConcurrentBag<string>();
            object lockObject = new object();
            Parallel.For(0, ExpData.HatTransform.Count, i =>
            {
                Curve HatShape = new Einstein.HatTile("Outline").PreviewShape;
                
                var AdjustTS = Transform.Translation(Adjust);
                var Scale = Transform.Scale(Point3d.Origin, Hatsize);
                var Final = Translation * AdjustTS * Scale * ExpData.HatTransform[i];
                HatShape.Transform(Final);
                var Pt = (HatShape as PolylineCurve).ToPolyline().CenterPoint();
                for (int j = 0; j < this.Regions.Count; j++)
                {
                    if (Regions[j].Contains(Pt, Plane.WorldXY, 0.1) == PointContainment.Inside)
                    {
                        Seq.Add(i);
                        TS.Add(ExpData.HatTransform[i]);
                        Pts.Add(Pt);
                        HatLabels.Add(ExpData.HatLabel[i]);
                    }
                }
            });
            var NewExpData = (ExpensionData) ExpData.Clone();
            lock (lockObject)
            {
                var indexes = Enumerable.Range(0, Seq.Count).ToArray();
                var sortedTS = TS.ToArray();
                var sortedPts = Pts.ToArray();
                var sortedLabel = HatLabels.ToArray();
                //Array.Sort(Seq.ToArray(), sortedTS);
                //Array.Sort(Seq.ToArray(), sortedPts);
                //Array.Sort(Seq.ToArray(), sortedLabel);
                NewExpData.SetTransform(sortedTS.ToList());
                NewExpData.SetTransformPoint(sortedPts.ToList());
                NewExpData.SetLabels(sortedLabel.ToList());
            }
            return NewExpData;
        }
    }

}
