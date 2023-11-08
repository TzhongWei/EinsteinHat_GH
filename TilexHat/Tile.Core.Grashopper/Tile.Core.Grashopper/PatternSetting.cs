using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino.UI.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tile.Core.Grashopper
{
    public class PatternSetting : GH_Component
    {
        public PatternSetting():base("PatternSetting", "Pattern",
            "This setting is filling the patterns for einstein block", "Einstein", "Einstein")
        { }

        public override Guid ComponentGuid => new Guid("AF1CCF18-E2F4-4CE5-9354-CF5CD1FF31A5");
        protected override Bitmap Icon => Properties.Resources.Pattern_setting_2;
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Label", "L", "The labels for the hats", GH_ParamAccess.item);
            pManager.AddNumberParameter("Scale", "S", "Scale setting for preview, which will not impact the final size", GH_ParamAccess.item, 1);
            pManager.AddBooleanParameter("Frame", "F", "Set the frame for the hats", GH_ParamAccess.item, false);
            pManager.AddGeometryParameter("Shape", "Sp", "The shape setting for the blocks", GH_ParamAccess.list);
            pManager[3].Optional = true;
            pManager.AddBooleanParameter("ColourFromObject", "CfO", "The colour source setting from the object or layer", GH_ParamAccess.item, false);
            
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGeometryParameter("Preview", "Pre", "Preview the shape frame faciliting configuring", GH_ParamAccess.item);
            pManager.AddGenericParameter("PatternSetting", "PSet", "The pattern setting for the hat conveying to the permuting class", GH_ParamAccess.item);
        }
        public override void AddedToDocument(GH_Document document)
        {
            base.AddedToDocument(document);
            //Add Value List
            int[] stringID = new int[] { 0 };

            for (int i = 0; i < stringID.Length; i++)
            {
                Param_String in0str = Params.Input[stringID[i]] as Param_String;
                if (in0str == null || in0str.SourceCount > 0 || in0str.PersistentDataCount > 0) return;
                Attributes.PerformLayout();
                int x = (int)in0str.Attributes.Pivot.X - 250;
                int y = (int)in0str.Attributes.Pivot.Y - 10;
                GH_ValueList valueList = new GH_ValueList();

                valueList.CreateAttributes();
                valueList.Attributes.Pivot = new System.Drawing.PointF(x, y);
                valueList.ListItems.Clear();
                if (i == 0)
                {
                    List<GH_ValueListItem> Type = new List<GH_ValueListItem>()
                    {
                    new GH_ValueListItem("All", "0"),
                    new GH_ValueListItem("H", "1"),
                    new GH_ValueListItem("H1", "2"),
                    new GH_ValueListItem("T", "3"),
                    new GH_ValueListItem("P", "4"),
                    new GH_ValueListItem("F", "5")
                    };
                    valueList.ListItems.AddRange(Type);
                    document.AddObject(valueList, false);
                    in0str.AddSource(valueList);
                }
            }
            
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double scale = double.NaN;
            string Option = "";
            bool Frame = false;
            List<GeometryBase> shapes = new List<GeometryBase>();
            List<IGH_GeometricGoo> Goo = new List<IGH_GeometricGoo>();
            bool ColourFromObject = false;
            DA.GetData(0, ref Option);
            DA.GetData(1, ref scale);
            DA.GetData(2, ref Frame);
            DA.GetData("ColourFromObject", ref ColourFromObject);

            switch (Option)
            {
                case ("0"):
                    Option = "all";
                    break;
                case ("1"):
                    Option = "h";
                    break;
                case ("2"):
                    Option = "h1";
                    break;
                case ("3"):
                    Option = "t";
                    break;
                case ("4"):
                    Option = "p";
                    break;
                case ("5"):
                    Option = "f";
                    break;
                default:
                    Option = "all";
                    break;
            }

            DA.GetDataList(3, Goo);

            try
            {
                scale = scale <= 0 ? 1 : scale;

                AddPatternOption Pattern = null;
                if (Goo.Count <= 0)
                {
                    Pattern = new AddPatternOption(Option, new List<Guid>(), true, scale, ColourFromObject);
                    DA.SetData(1, Pattern);
                }
                else
                {
                    List<System.Guid> guids = new List<Guid>();
                    for (int i = 0; i < Goo.Count; i++)
                        guids.Add(Goo[i].ReferenceID);
                    Pattern = new AddPatternOption(Option, guids, Frame, scale, ColourFromObject);
                    DA.SetData(1, Pattern);
                }
            }
            catch
            {
                List<GeometryBase> GB = new List<GeometryBase>();
                DA.GetDataList(3, GB);
                scale = scale <= 0 ? 1 : scale;

                AddPatternOption Pattern = null;
                if (GB.Count <= 0 || GB[0] == null)
                {
                    Pattern = new AddPatternOption(Option, new List<GeometryBase>(), Frame, scale);
                    DA.SetData(1, Pattern);
                }
                else
                {
                    Pattern = new AddPatternOption(Option, GB, Frame, scale);
                    DA.SetData(1, Pattern);
                }
            }
            

            DA.SetData(0, AddPatternOption.PreviewShape(scale));
        }
    }
}
