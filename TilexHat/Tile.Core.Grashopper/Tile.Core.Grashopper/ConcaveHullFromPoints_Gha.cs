using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Special;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using Tile.Core.Hull;

namespace Tile.Core.Grashopper
{
    public class ConcaveHullFromPoints_Gha : GH_Component
    {
        public ConcaveHullFromPoints_Gha() : base(
            "ConcaveHullFromPoints", "ConcaveHull",
            "Create concave hull from a point list",
            "Einstein", "Einstein") { }

        public override Guid ComponentGuid => new Guid("{223EF220-3E5F-4514-85B1-B28EB6BD509F}");

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "P", "A points List to create a concave hull", GH_ParamAccess.list);
            pManager.AddNumberParameter("Alpha", "A", "A number to control the threshold to break the segments which shouldn't be valued", GH_ParamAccess.item, 1.1);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("ConcaveMesh", "M", "The Concave mesh", GH_ParamAccess.item);
            pManager.AddPointParameter("OuterPoints", "Pts", "The outer points touch the hull", GH_ParamAccess.list);
            pManager.AddCurveParameter("Profile", "Crv", "The hull profile", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Indice", "I", "The indice of selected points of the hull", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Point3d> Pts = new List<Point3d>();
            double Alpha = 1.1;
            DA.GetDataList(0, Pts);
            DA.GetData(1, ref Alpha);
            var HullObject = Concave_Hull.ConcaveHull2D(Pts, Alpha);
            DA.SetData(0, HullObject.GetMesh);
            DA.SetDataList(1, HullObject.GetPoints);
            DA.SetData(2, HullObject.GetPolyCurve);
            DA.SetDataList(3, HullObject.GetProfileIndex);
        }
        protected override Bitmap Icon => base.Icon;
    }
}
