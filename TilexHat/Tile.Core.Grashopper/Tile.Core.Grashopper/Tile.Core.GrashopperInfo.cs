using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace Tile.Core.Grashopper
{
    public class Tile_Core_GrashopperInfo : GH_AssemblyInfo
    {
        public override string Name => "Tile.Core.Grashopper";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("907C432A-2EAC-495B-9732-03E13CD60EA2");

        //Return a string identifying you or your company.
        public override string AuthorName => "";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";
    }
}