using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Special;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Tile.Core.Hull;

namespace Tile.Core.Grashopper
{
    public class ConvexHullFromCrvs_Gha : GH_Component
    {
        public ConvexHullFromCrvs_Gha() : base(
            "ConvexHullFromCurves", "ConvexHull", 
            "Create convex hull from a curve list", 
            "Einstein", "Einstein")
        { }
        public override Guid ComponentGuid => new Guid("{E5235988-4CBA-4589-B3E9-166E48F6B17E}");
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curves", "Crvs", "Generate convex hull from curves", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Tolerance", "T", "The calibrate for the segments of the input curves", GH_ParamAccess.item, 3);
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("OuterPoints", "Pts", "The outer points touch the hull", GH_ParamAccess.list);
            pManager.AddCurveParameter("Profile", "Crv", "The hull profile", GH_ParamAccess.item);
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Curve> Crvs = new List<Curve>();
            DA.GetDataList(0, Crvs);
            int Tor = new int();
            DA.GetData(1, ref Tor);

            var Hull_Obj = Convex_Hull.ConvexHull2D(Crvs, Tor);
            DA.SetDataList(0, Hull_Obj.GetPoints);
            DA.SetData(1, Hull_Obj.GetPolyCurve);
        }
        protected override System.Drawing.Bitmap Icon => null;
    }
}
