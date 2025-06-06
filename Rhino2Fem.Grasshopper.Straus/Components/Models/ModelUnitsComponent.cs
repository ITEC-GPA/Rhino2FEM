using System;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Rhino2Fem.Core.Helper;
using Rhino2Fem.Core.Settings;
using Rhino2Fem.Grasshopper.Datatype;

namespace Rhino2Fem.Grasshopper.Straus.Components.Models
{
    public class ModelUnitsComponent : GH_Component
    {
        public ModelUnitsComponent()
            : base("Model Units", "Model Units", "Model Units", Core.Helper.Constants.CATEGORY_RHINO2STRAUS, Core.Helper.Constants.SUBCATEGORY_MODEL)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Length", "Length", "Length", GH_ParamAccess.item, 2);
            foreach (ModelUnitsModel.LengthUnitTypes v in Enum.GetValues(typeof(ModelUnitsModel.LengthUnitTypes)))
                ((Param_Integer)pManager[pManager.ParamCount - 1]).AddNamedValue(v.GetDescription(), (int)v);
            pManager.AddIntegerParameter("Force", "Force", "Force", GH_ParamAccess.item, 1);
            foreach (ModelUnitsModel.ForceUnitTypes v in Enum.GetValues(typeof(ModelUnitsModel.ForceUnitTypes)))
                ((Param_Integer)pManager[pManager.ParamCount - 1]).AddNamedValue(v.GetDescription(), (int)v);
            pManager.AddIntegerParameter("Temperature", "Temperature", "Temperature", GH_ParamAccess.item, 0);
            foreach (ModelUnitsModel.TemperatureUnitTypes v in Enum.GetValues(typeof(ModelUnitsModel.TemperatureUnitTypes)))
                ((Param_Integer)pManager[pManager.ParamCount - 1]).AddNamedValue(v.GetDescription(), (int)v);
            pManager.AddIntegerParameter("Heat", "Heat", "Heat", GH_ParamAccess.item, 0);
            foreach (ModelUnitsModel.HeatUnitTypes v in Enum.GetValues(typeof(ModelUnitsModel.HeatUnitTypes)))
                ((Param_Integer)pManager[pManager.ParamCount - 1]).AddNamedValue(v.GetDescription(), (int)v);
            pManager.AddNumberParameter("Tolerance", "Tolerance", "Tolerance", GH_ParamAccess.item, 0.001);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Model Units", "Model Units", "Model Units", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            int l = 0;
            int f = 0;
            int t = 0;
            int h = 0;
            double tol = 0;

            if (DA.GetData(0, ref l) && DA.GetData(1, ref f) && DA.GetData(2, ref t) && DA.GetData(3, ref h) && DA.GetData(4, ref tol))
            {
                ModelUnitsModel modelUnits = new ModelUnitsModel((ModelUnitsModel.ForceUnitTypes)f, (ModelUnitsModel.LengthUnitTypes)l, (ModelUnitsModel.HeatUnitTypes)t, (ModelUnitsModel.TemperatureUnitTypes)h, tol);
                DA.SetData(0, new GH_Units(modelUnits));
            }
        }

        //protected override Bitmap Icon => Resources.mgt_model_builder;

        public override Guid ComponentGuid => new Guid("96d26d68-578d-4804-a9ef-c6a878788b0b");
    }
}
