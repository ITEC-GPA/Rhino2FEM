using System;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino2Midas.Core.Attributes;
using Rhino2Midas.Core.Elements;
using Rhino2Midas.Grasshopper.Datatype;
using Rhino2Midas.Grasshopper.Properties;

namespace Rhino2Midas.Grasshopper.Components.Elements
{
    public class FrameElementComponent : GH_Component
    {
        public FrameElementComponent()
            : base("Frame element", "Frame element", "Frame element", "Rhino2Midas", "Model")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Frame line", "Frame line", "Frame line", GH_ParamAccess.item);
            pManager.AddGenericParameter("Frame section", "Frame section", "Frame section", GH_ParamAccess.item);
            pManager.AddGenericParameter("Material", "Material", "Material", GH_ParamAccess.item);
            pManager.AddAngleParameter("Angle (deg)", "Angle (deg)", "Angle (deg)", GH_ParamAccess.item, 0.0);
            pManager.AddGenericParameter("Groups", "Groups", "Groups", GH_ParamAccess.list);
            ((GH_ParamManager)pManager)[4].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Frame element", "Frame element", "Frame element", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Curve val = null;
            GH_FrameProperty gH_FrameProperty = null;
            Datatype.GH_Material gH_Material = null;
            List<GH_ElementGroup> groups = new List<GH_ElementGroup>();
            double angle = 0;

            if (!DA.GetData(0, ref val) || !DA.GetData(1, ref gH_FrameProperty) || !DA.GetData(2, ref gH_FrameProperty))
                return;
            DA.GetData(3, ref angle);
            DA.GetDataList(4, groups);

            NodeElementModel startNode = new NodeElementModel(val.PointAtStart.X, val.PointAtStart.Y, val.PointAtStart.Z, null, null, null);
            NodeElementModel endNode = new NodeElementModel(val.PointAtEnd.X, val.PointAtEnd.Y, val.PointAtEnd.Z, null, null, null);
            FrameElementModel frameElement = new FrameElementModel(startNode, endNode, gH_FrameProperty.Value, gH_Material.Value, Rhino.RhinoMath.ToRadians(angle));

            if (groups != null && groups.Count > 0)
            {
                List<ElementGroupModel> elementGroupModels = new List<ElementGroupModel>();
                for (int i = 0; i < groups.Count; i++)
                    elementGroupModels.Add(groups[i].Value);

                frameElement.Groups = elementGroupModels;
            }
            DA.SetData(0, new GH_FrameElement(frameElement));
        }

        //protected override Bitmap Icon => Resources.frame_element;

        public override Guid ComponentGuid => new Guid("99FE6257-8CFA-4DC2-9AEC-FB3AC0E1F8A3");
    }
}