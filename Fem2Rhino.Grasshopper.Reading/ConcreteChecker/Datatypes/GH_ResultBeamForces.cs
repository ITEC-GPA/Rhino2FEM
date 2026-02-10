using GPC.Model.Results;
using Grasshopper.Kernel.Types;

namespace Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes
{
    public class GH_ResultBeamForces : GH_Goo<ResultBeamForces>
    {
        public GH_ResultBeamForces(ResultBeamForces model)
        {
            Value = model;
        }

        public GH_ResultBeamForces()
        {

        }

        public override bool IsValid => true;

        public override string TypeName => "ResultBeamForces";

        public override string TypeDescription => throw new System.NotImplementedException();

        public override IGH_Goo Duplicate()
        {
            throw new System.NotImplementedException();
        }

        public override string ToString()
        {
            return "Name: " + Value.Name + ", \r\n" +
                "Axial Force: " + Value.N / 1000 + "kN" + ", \r\n" +
                "Shear X: " + Value.V1 / 1000 + "kN" + ", \r\n" +
                "Shear Y: " + Value.V2 / 1000 + "kN" + ", \r\n" +
                "Torsion: " + Value.T / 1000000 + "kNm" + ", \r\n" +
                "Bending X: " + Value.M1 / 1000000 + "kNm" + ", \r\n" +
                "Bending Y: " + Value.M2 / 1000000 + "kNm";
        }
    }
}
