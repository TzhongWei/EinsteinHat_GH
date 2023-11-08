using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Threading.Tasks;
using Rhino.Geometry;
using Grasshopper.Kernel;
using Grasshopper;
using Rhino;

namespace Tile.Core.Grashopper 
{
    public class EinsteinFittingComponent : GH_Component
    {
        public override Guid ComponentGuid => new Guid("{B6A31796-2688-47CE-A603-EC309FEDA889}");
        protected override Bitmap Icon => base.Icon;
        public EinsteinFittingComponent() : base(
            "EinsteinPatternFitting", "EinFitting",
            "This component is to fit the Einstein Hat pattern into given curve regions",
            "Einstein", "Einstein")
        { }          
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curves", "Crvs", "Curve regions for fitting", GH_ParamAccess.list);
            pManager.AddPointParameter("OriginPoint", "OriPt", "The origin point of the pattern", GH_ParamAccess.item, Point3d.Unset);
            pManager.AddBooleanParameter("SetBlock", "Set", "Set the hats into blocks in Rhino Environment", GH_ParamAccess.item);
            pManager.AddBooleanParameter("PlaceBlock", "Place", "configure the hat blocks", GH_ParamAccess.item);
            pManager.AddNumberParameter("Scale", "S", "The Scale setting of tiles", GH_ParamAccess.item, 1);
            pManager.AddGenericParameter("Patterns", "PSet", "The pattern setting", GH_ParamAccess.list);
            pManager[5].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Previewshape", "Pre", "Preview the arrangement of hat tiles", GH_ParamAccess.list);
            pManager.AddGenericParameter("ExpensionData", "Data", "The data for expension hat tiles", GH_ParamAccess.item);
        }
        Einstein_Forming FitEin = null;
        private List<Curve> _Crvs = new List<Curve>();
        private Point3d _OriPt = Point3d.Unset;
        private double _Scale = double.NaN;
        protected bool CompareCurve(List<Curve> Crv)
        {
            if (Crv.Count != _Crvs.Count)
                return true;
            else
            {
                for (int i = 0; i < Crv.Count; i++)
                {
                    if (Crv[i].GetLength() != _Crvs[i].GetLength())
                        return true;
                    if (Crv[i].PointAtEnd != _Crvs[i].PointAtEnd)
                        return true;
                    if (Crv[i].PointAtStart != _Crvs[i].PointAtStart)
                        return true;
                }
            }
            return false;
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Curve> Crv = new List<Curve>();
            DA.GetDataList(0, Crv);
            Point3d OriPt = Point3d.Unset;
            DA.GetData(1, ref OriPt);
            bool SetBlock = false, PlaceBlock = false;
            DA.GetData(2, ref SetBlock);
            DA.GetData(3, ref PlaceBlock);
            double Scale = double.NaN;
            DA.GetData(4, ref Scale);
            List<AddPatternOption> Options = new List<AddPatternOption>();
            DA.GetDataList(5, Options);

            if (Crv.Count <= 0 || Crv.Contains(null)) return;
            if (FitEin == null || SetBlock || _Scale != Scale || _OriPt != OriPt || CompareCurve(Crv))
            {
                _Scale = Scale;
                _OriPt = OriPt;
                _Crvs = new List<Curve>(Crv);
                this.FitEin = new Einstein_Forming(Crv, OriPt, Scale);
            }

            if (SetBlock)
            {
                if (Options.Count > 0)
                    FitEin.NewSetPatterns(Options);
                else
                    FitEin.NewSetFrame();
                FitEin.SetNewBlock(Name: GHShareValue.ApplyNames, Blockoverride: GHShareValue.OverrideBlock);
            }

            if (PlaceBlock && !SetBlock)
                FitEin.PlaceBlock();

            if (FitEin.HasFit)
            {
                var Preview = FitEin.PreviewShape();
                DA.SetDataList(0, Preview);
                DA.SetData(1, FitEin.FittingArrangement);
            }
        }
        public override void CreateAttributes()
        {
            m_attributes = new CustomUI.ButtonUIAttributes(this, "Update", FunctionToRunOnClick, "Click Here to start computation");
        }
        public void FunctionToRunOnClick()
        {
            if (this.FitEin != null)
            {
                this.FitEin.StartFitting();
                this.ExpireSolution(true);
            }
        }
    }
}
