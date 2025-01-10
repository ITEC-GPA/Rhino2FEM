using System;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Rhino2Midas.Core.ElementProperties;
using Rhino2Midas.Core.Helper;
using Rhino2Midas.Grasshopper.Datatype;

namespace Rhino2Midas.Grasshopper.Components.Materials
{
    public class MaterialComponent : GH_Component
    {
        public MaterialComponent()
            : base("Material", "Material", "Material", Helper.Constants.Tabname, Helper.Constants.Materials)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "Name", "Name", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Type", "Type", "Steel or Concrete", GH_ParamAccess.item);
            foreach (MaterialModel.MaterialTypes v in Enum.GetValues(typeof(MaterialModel.MaterialTypes)))
                ((Param_Integer)pManager[pManager.ParamCount - 1]).AddNamedValue(v.GetDescription(), (int)v);
            pManager.AddNumberParameter("Damping ratio", "Damping ratio", "Damping ratio", GH_ParamAccess.item, 0.05);
            pManager.AddNumberParameter("Modulus elasticity", "Modulus elasticity", "Modulus elasticity", GH_ParamAccess.item);
            pManager.AddNumberParameter("Poisson ratio", "Poisson ratio", "Poisson ratio", GH_ParamAccess.item, 0.3);
            pManager.AddNumberParameter("Thermal coefficent", "Thermal coefficent", "Thermal coefficent", GH_ParamAccess.item, 1.17E-05);
            pManager.AddNumberParameter("Density", "Density", "Density", GH_ParamAccess.item);
            pManager.AddNumberParameter("Mass", "Mass", "Density divided by gravity", GH_ParamAccess.item);
            ((GH_ParamManager)pManager)[2].Optional = true;
            ((GH_ParamManager)pManager)[4].Optional = true;
            ((GH_ParamManager)pManager)[5].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Material", "Material", "Material", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string name = "";
            int type = 0;
            double dampingRatio = 0.0;
            double modulusElasticity = 0.0;
            double poissonRatio = 0.0;
            double thermalCoefficient = 0.0;
            double density = 0.0;
            double mass = 0.0;

            if (DA.GetData(0, ref name) && DA.GetData(1, ref type))
            {
                if (DA.GetData(3, ref modulusElasticity) && DA.GetData(4, ref poissonRatio) && DA.GetData(5, ref thermalCoefficient) &&
                    DA.GetData(6, ref density) && DA.GetData(7, ref mass))
                {
                    MaterialModel material = new MaterialModel(name, (MaterialModel.MaterialTypes)type, dampingRatio, modulusElasticity, poissonRatio, thermalCoefficient, density, mass);
                    DA.SetData(0, new GH_Material(material));
                }
            }
        }

        //protected override Bitmap Icon => Resources.material;

        public override Guid ComponentGuid => new Guid("44B28612-9EB5-4C5F-880D-113B4A39921F");
    }
}
