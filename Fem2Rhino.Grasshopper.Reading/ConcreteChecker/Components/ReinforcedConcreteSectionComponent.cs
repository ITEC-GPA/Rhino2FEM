using Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes;
using GPC.Model.Sections.Concrete;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino2Fem.Core.Helper;
using System;
using System.Collections.Generic;

namespace Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Components
{
    public class ReinforcedConcreteSectionComponent : GH_Component
    {
        public ReinforcedConcreteSectionComponent()
            : base("Reinforced Concrete Section", "RCS", "Reinforced Concrete Section", Constants.CATEGORY_CHECKS, Constants.SUBCATEGORY_CHECKS_CONCRETECHECKER)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Section Name", "n", "The section name", GH_ParamAccess.item, "");
            pManager.AddBrepParameter("Surface", "S", "The section surface", GH_ParamAccess.item);
            pManager.AddGenericParameter("Concrete Material", "M", "The concrete material", GH_ParamAccess.item);
            pManager.AddGenericParameter("Rebar elements", "RE", "The rebar element list", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Reinforced Concrete Section", "RCS", "Reinforced Concrete Section", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string name = string.Empty;
            GH_Brep brep = null;
            GH_ConcreteMaterial concreteMaterial = null;
            List<GH_Rebar> rebarElements = new List<GH_Rebar>();

            if (DA.GetData(0, ref name) && DA.GetData(1, ref brep) && DA.GetData(2, ref concreteMaterial) && DA.GetDataList(3, rebarElements))
            {
                List<ReinforcedConcreteRebar> rebars = new List<ReinforcedConcreteRebar>();
                for (int i = 0; i < rebarElements.Count; i++)
                {
                    GH_Rebar rebarElement = rebarElements[i];
                    if (rebarElement != null && rebarElement.Value != null)
                        rebars.Add(rebarElement.Value);
                }

                //Test that all brep faces are coplanar
                Rhino.Geometry.Vector3d normal = Vector3d.Unset;

                GPC.Geometry.Shape2d shape = null;

                Brep b = brep.Value;

                foreach (BrepFace bf in b.Faces)
                {
                    if (!bf.IsPlanar())
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Brep faces must be planar");
                        return;
                    }
                    if (normal == Vector3d.Unset)
                    {
                        normal = bf.NormalAt(0, 0);
                        normal.Unitize();
                    }
                    else
                    {
                        Vector3d faceNorm = bf.NormalAt(0, 0);
                        faceNorm.Unitize();
                        if (Math.Abs(faceNorm * normal) < 0.999)
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Brep faces must be coplanar");
                    }

                    Transform tr = Transform.PlaneToPlane(new Plane(Point3d.Origin, normal), Plane.WorldXY);

                    foreach (BrepFace bface in b.Faces)
                    {
                        if (!bface.OuterLoop.To3dCurve().TryGetPolyline(out Polyline pl))
                        {
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Brep loops must be polylines");
                            return;
                        }
                        int lastIndex = pl.IsClosed ? pl.Count - 1 : pl.Count;
                        GPC.Geometry.Polygon2d fill = new GPC.Geometry.Polygon2d();
                        for (int i = 0; i < lastIndex; i++)
                        {
                            Point3d local = new Point3d(pl[i]);
                            local.Transform(tr);
                            fill.Add(local.X, local.Y);
                        }
                        List<GPC.Geometry.Polygon2d> holes = null;
                        foreach (BrepLoop bl in bface.Loops)
                        {
                            if (bl != bface.OuterLoop)
                            {
                                //is an hole
                                GPC.Geometry.Polygon2d hole;
                                if (!bl.To3dCurve().TryGetPolyline(out pl))
                                {
                                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Brep loops must be polylines");
                                    return;
                                }
                                lastIndex = pl.IsClosed ? pl.Count - 1 : pl.Count;
                                hole = new GPC.Geometry.Polygon2d();
                                for (int i = 0; i < lastIndex; i++)
                                {
                                    Point3d local = new Point3d(pl[i]);
                                    local.Transform(tr);
                                    hole.Add(local.X, local.Y);
                                }
                                if (holes == null) holes = new List<GPC.Geometry.Polygon2d>();
                                holes.Add(hole);
                            }
                        }
                        shape = new GPC.Geometry.Shape2d(fill, holes?.ToArray());
                    }
                }

                ReinforcedConcreteSection reinforcedConcreteSection = new ReinforcedConcreteSection(shape, concreteMaterial.Value, name);
                reinforcedConcreteSection.AddRebars(rebars);

                DA.SetData(0, new GH_ReinforcedConcreteSection(reinforcedConcreteSection));
            }
        }

        //protected override Bitmap Icon => Resources.material;

        public override Guid ComponentGuid => new Guid("f247cd41-e5cb-4a35-b9b8-e06b4c43afc6");
    }
}
