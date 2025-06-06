using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace Rhino2Fem.Midas.Grasshopper
{
    public class Rhino2Midas_GrasshopperInfo : GH_AssemblyInfo
    {
        public override string Name => "Rhino2Midas";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("a2892266-7163-4cc4-8bd0-971867fd6730");

        //Return a string identifying you or your company.
        public override string AuthorName => "Gabriele Pacini";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "gabrielepacini6293@gmail.com";

        //Return a string representing the version.  This returns the same version as the assembly.
        public override string AssemblyVersion => GetType().Assembly.GetName().Version.ToString();
    }
}