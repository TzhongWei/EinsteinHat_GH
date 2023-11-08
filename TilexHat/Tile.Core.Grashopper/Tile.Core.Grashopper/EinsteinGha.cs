using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Special;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Tile.Core;

namespace Tile.Core.Grashopper
{
    public class EinsteinComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public EinsteinComponent()
          : base("EinsteinCore", "EinCore",
            "This component is used to generate the einstein hat patterns and format, which displays in curves",
            "Einstein", "Einstein")
        {
        }
        /// <summary>
        /// Create two lists for selection
        /// </summary>
        /// <param name="document"></param>
        public override void AddedToDocument(GH_Document document)
        {
            base.AddedToDocument(document);
            //Add Value List
            int[] stringID = new int[] { 2, 3 };

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
                    new GH_ValueListItem("H", "0"),
                    new GH_ValueListItem("T", "1"),
                    new GH_ValueListItem("P", "2"),
                    new GH_ValueListItem("F", "3")
                    };
                    valueList.ListItems.AddRange(Type);
                    document.AddObject(valueList, false);
                    in0str.AddSource(valueList);
                }
                else
                {
                    List<GH_ValueListItem> Type = new List<GH_ValueListItem>()
                    {
                    new GH_ValueListItem("MetaTiles", "0"),
                    new GH_ValueListItem("Hat", "1"),
                    new GH_ValueListItem("H", "2"),
                    new GH_ValueListItem("H1", "3"),
                    new GH_ValueListItem("T", "4"),
                    new GH_ValueListItem("P", "5"),
                    new GH_ValueListItem("F", "6"),
                    new GH_ValueListItem("All", "7")
                    };
                    valueList.ListItems.AddRange(Type);
                    document.AddObject(valueList, false);
                    in0str.AddSource(valueList);
                }
            }

        }
        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Reset", "R", "Reset the pattern", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Execute", "E", "Execute the program, and expend the patterns", GH_ParamAccess.item);
            pManager.AddTextParameter("InitialType", "I", "Initial Metatile for patterns expention", GH_ParamAccess.item);
            pManager.AddTextParameter("Display", "D", "Display metatiles and hat type", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Geom", "C", "The patterns output as a set of curves", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Level", "L", "The level of the program", GH_ParamAccess.item);
            pManager.AddGenericParameter("Data", "D", "The data from this program, which displays the structure of hats", GH_ParamAccess.list);
            pManager.AddGenericParameter("EinsteinData", "Ein", "The einstein class as output to permute and customise the hat patterns", GH_ParamAccess.item);
        }
        int Count = 0;
        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool Reset = false;
            bool Execute = false;
            DA.GetData(0, ref Reset);
            DA.GetData(1, ref Execute);
            string TypeStr = "";
            DA.GetData(2, ref TypeStr);
            int Type = int.Parse(TypeStr);
            string DistStr = "";
            DA.GetData(3, ref DistStr);
            int DisplayType = int.Parse(DistStr);

            var Einstein_Hat = new Einstein();
            if (Reset)
                Count = 0;
            if (Execute)
                Count++;

            Einstein_Hat.reset();
            for (int i = 0; i < Count; i++)
                Einstein_Hat.NextIteration();

            List<KeyValuePair<string, Curve>> TilesPair = new List<KeyValuePair<string, Curve>>();
            Einstein.MetaTile metatile = null;
            switch (Type)
            {
                case 0:
                    TilesPair = Einstein_Hat.Draw(Einstein.Type.H);
                    metatile = Einstein_Hat.Tiles[0];
                    break;
                case 1:
                    TilesPair = Einstein_Hat.Draw(Einstein.Type.T);
                    metatile = Einstein_Hat.Tiles[1];
                    break;
                case 2:
                    TilesPair = Einstein_Hat.Draw(Einstein.Type.P);
                    metatile = Einstein_Hat.Tiles[2];
                    break;
                case 3:
                    TilesPair = Einstein_Hat.Draw(Einstein.Type.T);
                    metatile = Einstein_Hat.Tiles[3];
                    break;
            }


            List<Curve> MetaTiles = new List<Curve>();
            List<Curve> H = new List<Curve>();
            List<Curve> H1 = new List<Curve>();
            List<Curve> T = new List<Curve>();
            List<Curve> P = new List<Curve>();
            List<Curve> F = new List<Curve>();
            for (int i = 0; i < TilesPair.Count; i++)
            {
                switch (TilesPair[i].Key)
                {
                    case "Meta":
                        MetaTiles.Add(metatile.PreviewShape);
                        MetaTiles.Add(TilesPair[i].Value);
                        break;
                    case "H":
                        H.Add(TilesPair[i].Value);
                        break;
                    case "H1":
                        H1.Add(TilesPair[i].Value);
                        break;
                    case "T":
                        T.Add(TilesPair[i].Value);
                        break;
                    case "P":
                        P.Add(TilesPair[i].Value);
                        break;
                    case "F":
                        F.Add(TilesPair[i].Value);
                        break;
                }
            }

            List<Curve> Display = new List<Curve>();
            switch (DisplayType)
            {
                case 0:
                    Display.AddRange(MetaTiles);
                    break;
                case 1:
                    Display.AddRange(H);
                    Display.AddRange(H1);
                    Display.AddRange(T);
                    Display.AddRange(P);
                    Display.AddRange(F);
                    break;
                case 2:
                    Display.AddRange(H);
                    break;
                case 3:
                    Display.AddRange(H1);
                    break;
                case 4:
                    Display.AddRange(T);
                    break;
                case 5:
                    Display.AddRange(P);
                    break;
                case 6:
                    Display.AddRange(F);
                    break;
                case 7:
                    Display.AddRange(MetaTiles);
                    Display.AddRange(H);
                    Display.AddRange(H1);
                    Display.AddRange(T);
                    Display.AddRange(P);
                    Display.AddRange(F);
                    break;
            }

            DA.SetDataList(0, Display);
            DA.SetData(1, Einstein_Hat.Level);
            DA.SetDataList(2, TilesPair);
            DA.SetData(3, Einstein_Hat);
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// You can add image files to your project resources and access them like this:
        /// return Resources.IconForThisComponent;
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Properties.Resources.Einstein_core_2;

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid => new Guid("73C4CDF0-F1D4-4793-A7AF-D03B6EE94B1F");
    }
}