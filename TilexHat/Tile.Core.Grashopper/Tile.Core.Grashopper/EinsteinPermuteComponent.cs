using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Threading.Tasks;
using Rhino.Geometry;
using Grasshopper.Kernel;
using Rhino;

namespace Tile.Core.Grashopper
{
    public class EinsteinPermuteComponen : GH_Component
    {
        public EinsteinPermuteComponen() : base("EinsteinPatternPatch",
            "EinPatch", "This component is to match the pattern based on einstein core",
            "Einstein", "Einstein")
        { }

        public override Guid ComponentGuid => new Guid("495759FF-8C1D-4EFE-9041-F23A524BBE96");
        protected override Bitmap Icon => Properties.Resources.Pattern_patch_2;
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Einstein", "EinCore", "The einstein class provides the positions and data for pattern arrangement", GH_ParamAccess.item);
            pManager.AddPointParameter("OriginPoint", "OriPt", "The origin point of the pattern", GH_ParamAccess.item, new Point3d(0,0,0));
            pManager.AddBooleanParameter("SetBlock", "Set", "Set the hats into blocks in Rhino Environment", GH_ParamAccess.item);
            pManager.AddBooleanParameter("PlaceBlock", "Place", "configure the hat blocks", GH_ParamAccess.item);
            pManager.AddNumberParameter("Scale", "S", "The Scale setting of tiles", GH_ParamAccess.item, 1);
            pManager.AddGenericParameter("Patterns", "PSet", "The pattern setting", GH_ParamAccess.list);
            pManager[5].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGeometryParameter("PreviewSize", "Pre", "This output provides a preview block", GH_ParamAccess.list);
        }
        //bool JustSetBlock = false;
        Einstein_Resize Resize = null;
        double _Scale = double.NaN;
        Point3d _OriPt = Point3d.Origin;
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Einstein Ein = new Einstein();
            DA.GetData(0, ref Ein);
            Point3d OriPt = new Point3d(0, 0, 0);
            DA.GetData(1, ref OriPt);
            bool SetBlock = false, PlaceBlock = false;
            DA.GetData(2, ref SetBlock);
            DA.GetData(3, ref PlaceBlock);
            double Scale = double.NaN;
            DA.GetData(4, ref Scale);
            List<AddPatternOption> Options = new List<AddPatternOption>();
            DA.GetDataList(5, Options);

            if (Resize == null || SetBlock || double.IsNaN(_Scale) || _Scale != Scale || _OriPt != OriPt)
            {
                _Scale = Scale;
                _OriPt = OriPt;
                Resize = new Einstein_Resize(Scale * 2, OriPt);
                Resize.SetTile = Ein;
            }

            if (SetBlock)
            {
                if (Options.Count > 0)
                {
                    Resize.NewSetPatterns(Options);
                    //Resize.SetPatterns(Options);
                }
                else
                {
                    Resize.NewSetFrame();
                }
                Resize.SetNewBlock(Name: GHShareValue.ApplyNames ,Blockoverride: GHShareValue.OverrideBlock);
            }


            
            if (PlaceBlock && !SetBlock)
                Resize.PlaceBlock(Ein);

            var Preview = Resize.PreviewShape();
            DA.SetDataList(0, Preview);
        }
    }
}
