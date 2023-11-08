using System;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Special;
using Rhino.Geometry;
using System.Collections.Generic;
using System.Drawing;
using Tile.Core.Hull;
using System.Linq;

namespace Tile.Core.Grashopper
{
    internal static class GHShareValue
    {
        public static DataTree<string> H_BlockName { get 
            {
                var Name = new DataTree<string>();
                for (int i = 0; i < 5; i++)
                {
                    Name.AddRange(Util.Util.BlockManagers[i].BlockIdAndName.Keys, new Grasshopper.Kernel.Data.GH_Path(i));
                }
                return Name;
            } 
        }
        public static List<string> ApplyNames { get; set; } = new List<string>();
        public static bool OverrideBlock { get; set; } = false;
        private static object RemoveAllBlock(Util.Util.BlockIDandNameManager Manager, object sender)
        {
            Manager.RemoveAll();
            return true;
        }
        public static void RemoveAllBlock()
        {
            Util.Util.BlockManagers.SystemSetting(RemoveAllBlock, out _);
        }
    }
}
