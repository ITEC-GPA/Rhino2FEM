using Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes;
using GPC.Model.Results;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino2Fem.Core.Helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Components
{
    public class PostProcessFailureDomain3DComponent : GH_Component
    {
        public PostProcessFailureDomain3DComponent()
            : base("3D Failure Domain Analysis", "FDA", "3D Failure domain Analysis", Constants.CATEGORY_CHECKS, Constants.SUBCATEGORY_CHECKS_CONCRETECHECKER)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("3D Failure Domain Analysis", "FDA", "3D Failure Domain Analysis", GH_ParamAccess.item);
            pManager.AddGenericParameter("Forces", "F", "Forces", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("To Excel", "TE", "To Excel", GH_ParamAccess.list);
            pManager.AddPointParameter("External Force", "EF", "External Force", GH_ParamAccess.list);
            pManager.AddPointParameter("Resistance Force", "RF", "Resistance Force", GH_ParamAccess.list);
            pManager.AddNumberParameter("Working Ratio", "wR", "Working Ratio", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_FailureDomain3DResult gH_FailureDomain3DResult = null;
            List<GH_ResultBeamForces> gH_ResultBeamForces = new List<GH_ResultBeamForces>();

            if (DA.GetData(0, ref gH_FailureDomain3DResult) && DA.GetDataList(1, gH_ResultBeamForces))
            {
                ResultBeamForces[] resultBeamForces = new ResultBeamForces[gH_ResultBeamForces.Count];
                List<Point3d> externalPointsRhino = new List<Point3d>();
                string[] toExcelBuffer = new string[gH_ResultBeamForces.Count];

                for (int i = 0; i < gH_ResultBeamForces.Count; i++)
                {
                    resultBeamForces[i] = new ResultBeamForces(gH_ResultBeamForces[i].Value.N, gH_ResultBeamForces[i].Value.V1,
                        gH_ResultBeamForces[i].Value.V2, gH_ResultBeamForces[i].Value.T, gH_ResultBeamForces[i].Value.M1, gH_ResultBeamForces[i].Value.M2,
                        gH_ResultBeamForces[i].Value.CoordinateSystem, i, gH_ResultBeamForces[i].Value.Name);
                    externalPointsRhino.Add(new Point3d(gH_ResultBeamForces[i].Value.M1 / 1000000.0, gH_ResultBeamForces[i].Value.M2 / 1000000.0, gH_ResultBeamForces[i].Value.N / 1000.0));
                    toExcelBuffer[i] = $"{i};{gH_ResultBeamForces[i].Value.Name};{gH_ResultBeamForces[i].Value.N / 1000};{gH_ResultBeamForces[i].Value.M1 / 1000000};{gH_ResultBeamForces[i].Value.M2 / 1000000};";
                }

                GPC.Checkers.Concrete.Results.FailureDomain.FailureDomainPoint[] points = gH_FailureDomain3DResult.Value.AddForces(resultBeamForces);
                GH_Point[] resistancePointsRhino = new GH_Point[points.Length];
                for (int i = 0; i < points.Length; i++)
                {
                    resistancePointsRhino[i] = new GH_Point(new Point3d(points[i].Point.X / 1000000, points[i].Point.Y / 1000000, points[i].Point.Z / 1000));
                    toExcelBuffer[i] = string.Concat(toExcelBuffer[i], $"{Math.Round(resistancePointsRhino[i].Value.Z, 2)};{Math.Round(resistancePointsRhino[i].Value.X, 2)};{Math.Round(resistancePointsRhino[i].Value.Y, 2)};");
                }
                for (int i = 0; i < points.Length; i++)
                {
                    points[i].CalculateWorkingRatio(gH_FailureDomain3DResult.Value.FailureAnalysisType, gH_ResultBeamForces[i].Value, 1000000, 1000);
                    toExcelBuffer[i] = string.Concat(toExcelBuffer[i], $"{Math.Round(points[i].WorkingRatio, 3)}");
                }

                double[] wr = points.Select(p => p.WorkingRatio).ToArray();

                DA.SetDataList(0, toExcelBuffer);
                DA.SetDataList(1, externalPointsRhino);
                DA.SetDataList(2, resistancePointsRhino);
                DA.SetDataList(3, wr);
            }
        }

        //protected override Bitmap Icon => Resources.material;

        public override Guid ComponentGuid => new Guid("96102691-a47f-4cb5-a411-db638040b01e");

        public override GH_Exposure Exposure => GH_Exposure.primary;
    }
}
