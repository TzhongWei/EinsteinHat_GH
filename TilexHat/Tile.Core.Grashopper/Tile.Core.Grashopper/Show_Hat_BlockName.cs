using Grasshopper.Kernel;
using Grasshopper;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using Tile.Core.Hull;
using Tile.Core.Grashopper.CustomUI;
using Tile.Core;


namespace Tile.Core.Grashopper
{
    public class Show_Hat_BlockName : GH_Component
    {
        public Show_Hat_BlockName() : base(
            "Show_All_HatName", "Show_Hat", 
            "This Component output all current defined hat blocknames in Rhino", 
            "Einstein", "Einstein")
        { }
        public override Guid ComponentGuid => new Guid("{1620A618-E389-459D-A680-DA1854F74228}");

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("IsOverwrite", "O", "A setting for overwrite the exist blocks in rhino", GH_ParamAccess.item, false);
            pManager.AddTextParameter("PermuteBlockName", "P", "A list of block names that would be used for permuting", GH_ParamAccess.list);
            pManager.AddTextParameter("RemoveName", "Del", "A list of remove the name of blocks", GH_ParamAccess.list);
            pManager.AddBooleanParameter("DeleteAll", "DelAll", "Delete all block", GH_ParamAccess.item);
            pManager[0].Optional = true;
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("BlockName", "Name", "The hat block names stored in rhino", GH_ParamAccess.tree);
        }
        DataTree<string> BlockName = new DataTree<string>();
        List<string> RemoveName = new List<string>();
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool Set = false;
            DA.GetData(0, ref Set);
            List<string> PermuteName = new List<string>();
            bool RemoveAll = false;
            DA.GetDataList("PermuteBlockName", PermuteName);
            DA.GetDataList("RemoveName", RemoveName);
            DA.GetData("DeleteAll", ref RemoveAll);

            BlockName = GHShareValue.H_BlockName;
            GHShareValue.OverrideBlock = Set;
            GHShareValue.ApplyNames = PermuteName;
            if (RemoveAll)
            {
                GHShareValue.RemoveAllBlock();
            }

            DA.SetDataTree(0, BlockName);
        }
        protected override Bitmap Icon => base.Icon;

        //Button
        public override void CreateAttributes()
        {
            m_attributes = new CustomUI.ButtonUIAttributes(this, "Update", FunctionToRunOnClick, "Update Block Information");
        }

        public void FunctionToRunOnClick()
        {
            if (RemoveName.Count > 0)
            {
                Util.Util.Remove(RemoveName);
                RemoveName.Clear();
            }
            this.ExpireSolution(true);
            //System.Windows.Forms.MessageBox.Show("Block Data is updated.");
        }
    }
}
