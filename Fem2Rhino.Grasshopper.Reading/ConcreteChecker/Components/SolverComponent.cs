using Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes;
using GPC.Checkers.Concrete.SectionSolvers;
using GPC.Geometry;
using GPC.Model.Data.Steel;
using GPC.Model.Materials;
using GPC.Model.Sections.Concrete;
using GPC.Model.Sections.Rebar;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Rhino.Geometry;
using Rhino2Fem.Core.Helper;
using System;
using System.Collections.Generic;

namespace Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Components
{
    public class SolverComponent : GH_Component
    {
        public SolverComponent()
            : base("Concrete Solver", "CS", "Concrete Solver", Constants.CATEGORY_CHECKS, Constants.SUBCATEGORY_CHECKS_CONCRETECHECKER   )
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Reinforced Concrete Section", "RCS", "The reinforced concrete section", GH_ParamAccess.item);
            pManager.AddGenericParameter("Standard", "S", "The standard", GH_ParamAccess.item);
            pManager.AddGenericParameter("Options", "O", "The analysis options", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Concrete Solver", "CS", "Concrete Solver", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_ReinforcedConcreteSection gH_ReinforcedConcreteSection = null;
            GH_Standard gH_Standard = null;

            if (DA.GetData(0, ref gH_ReinforcedConcreteSection) && DA.GetData(1, ref gH_Standard) && DA.GetData(0, ref location))
            {

                var solver = new SectionSolverModelCode2010(gH_ReinforcedConcreteSection.Value, options, gH_Standard.Value, gH_ReinforcedConcreteSection.Value.Centroid, considerTensileConcrete);

                DA.SetData(0, new GH_Rebar(reinforcedConcreteRebar));
            }
        }

        //protected override Bitmap Icon => Resources.material;
        
        public override Guid ComponentGuid => new Guid("411284c2-923a-4475-94a5-8405ece71ce0");
    }
}
