using System;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Rhino2Fem.Core.Base;
using Rhino2Fem.Core.ElementProperties;
using Rhino2Fem.Core.Helper;
using Rhino2Fem.Grasshopper.Datatype;

namespace Rhino2Fem.Grasshopper.Straus.Components.ElementProperties
{
    public class AreaPropertyComponent : GH_Component
    {
        public AreaPropertyComponent()
            : base("Area Property", "Area Property", "Area Property", Core.Helper.Constants.CATEGORY_RHINO2STRAUS, Core.Helper.Constants.SUBCATEGORY_ELEMENTPROPERTIES)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "Name", "Name", GH_ParamAccess.item);
            pManager.AddGenericParameter("Section", "Section", "Section", GH_ParamAccess.item);
            pManager.AddGenericParameter("Material", "Material", "Material", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Area Property", "Area Property", "Area Property", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string name = "";
            GH_AreaThickness gH_AreaThickness= null;
            GH_Material gH_Material = null;

            if (DA.GetData(0, ref name) && DA.GetData(1, ref gH_AreaThickness) && DA.GetData(2, ref gH_Material))
            {
                AreaPropertyModel frameProperty = new AreaPropertyModel(name, gH_Material.Value, gH_AreaThickness.Value) { PropertyType = AreaPropertyModel.PlatePropertyType.ShellThick };
                DA.SetData(0, new GH_AreaProperty(frameProperty));
            }
        }

        //protected override Bitmap Icon => Resources.frame_property_tee;

        public override Guid ComponentGuid => new Guid("3edcae0b-6f75-48b9-94b2-538d8e714262");
    }
}