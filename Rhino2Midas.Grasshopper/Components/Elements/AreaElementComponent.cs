using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino2Fem.Core.Attributes;
using Rhino2Fem.Core.Base;
using Rhino2Fem.Core.Elements;
using Rhino2Fem.Grasshopper.Datatype;

namespace Rhino2Fem.Midas.Grasshopper.Components.Elements
{
    public class AreaElementComponent : GH_Component
    {

        public AreaElementComponent()
            : base("Area element", "Area element", "Area element", Core.Helper.Constants.CATEGORY_RHINO2MIDAS, Core.Helper.Constants.SUBCATEGORY_ELEMENTS)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Area", "Area", "Area", GH_ParamAccess.item);
            pManager.AddGenericParameter("Area thickness", "Area thickness", "Area thickness", GH_ParamAccess.item);
            pManager.AddGenericParameter("Material", "Material", "Material", GH_ParamAccess.item);
            pManager.AddAngleParameter("Angle (deg)", "Angle (deg)", "Angle (deg)", GH_ParamAccess.item, 0);
            pManager.AddGenericParameter("Groups", "Groups", "Groups", GH_ParamAccess.list);
            ((GH_ParamManager)pManager)[4].Optional = true;
            pManager.AddIntegerParameter("Area ID", "Area ID", "Area ID", GH_ParamAccess.item, ModelObjectId.UNASSIGNED);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Area element", "Area element", "Area element", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh mesh = new Mesh();
            GH_AreaThickness areaThickness = new GH_AreaThickness();
            Rhino2Fem.Grasshopper.Datatype.GH_Material material = new Rhino2Fem.Grasshopper.Datatype.GH_Material();
            List<IGH_Goo> list = new List<IGH_Goo>();
            double angle = 0.0;
            List<GH_ElementGroup> groups = new List<GH_ElementGroup>();
            int id = 0;

            if (!DA.GetData(0, ref mesh) || !DA.GetData(1, ref areaThickness) || !DA.GetData(2, ref material))
                return;
            DA.GetData(3, ref angle);
            DA.GetDataList(4, groups);
            DA.GetData(5, ref id);

            int num = mesh.Vertices.Count;
            if (num < 3.0 || num > 4.0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Area can only have 3 or 4 points!");
                return;
            }

            List<NodeElementModel> listBuffer = new List<NodeElementModel>();
            for (int i = 0; i < num; i++)
                listBuffer.Add(new NodeElementModel(mesh.Vertices[i]));

            AreaElementModel areaElement = new AreaElementModel(listBuffer, areaThickness.Value, material.Value, angle) { Id = id }; 
            if (groups != null && groups.Count > 0)
            {
                List<ElementGroupModel> elementGroupModels = new List<ElementGroupModel>();
                for (int i = 0; i < groups.Count; i++)
                    elementGroupModels.Add(groups[i].Value);

                areaElement.Groups = elementGroupModels;
            }

            DA.SetData(0, new GH_AreaElement(areaElement));
        }

        //protected override Bitmap Icon => Resources.area_element;

        public override Guid ComponentGuid => new Guid("D14C96EB-F7EF-4410-A133-0A7598FED072");
    }
}
