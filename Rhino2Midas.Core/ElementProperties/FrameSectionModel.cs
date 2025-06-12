using Rhino.Collections;
using Rhino.Geometry;
using Rhino2Fem.Core.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Rhino2Fem.Core.ElementProperties
{
    public class FrameSectionModel : ModelObjectId, IEquatable<FrameSectionModel>
    {
        public enum Types
        {
            DBUSER,
            COMPOSITE_I,
        }

        public enum SectionGeometryTypes
        {
            [Description("Rectangular/Solid Box")]
            SB,
            [Description("Cable/Solid Round")]
            SR,
            [Description("Pipe/CHS Section")]
            P,
            [Description("L Section")]
            L,
            [Description("C Section")]
            C,
            [Description("H Section")]
            H,
            [Description("T Section")]
            T,
            [Description("RHS/Box Section")]
            B,
            [Description("Generic Section")]
            Generic,
        }

        public enum OffsetTypes
        {
            LT,
            CT,
            RT,
            LC,
            CC,
            RC,
            LB,
            CB,
            RB,
        }

        public SectionGeometryTypes SectionGeometryType { get; set; }
        public Types Type { get; set; }
        public OffsetTypes Offset { get; set; }

        public double Dimension1 { get; set; }
        public double Dimension2 { get; set; }
        public double Dimension3 { get; set; }
        public double Dimension4 { get; set; }
        public double Dimension5 { get; set; }
        public double Dimension6 { get; set; }
        public double Dimension7 { get; set; }
        public double Dimension8 { get; set; }
        public double Dimension9 { get; set; }
        public double Dimension10 { get; set; }

        public double CompositeData1 { get; set; }
        public double CompositeData2 { get; set; }
        public double CompositeData3 { get; set; }
        public double CompositeData4 { get; set; }
        public double CompositeData5 { get; set; }

        public double SectionArea { get; set; }
        public double Ixx { get; set; }
        public double Iyy { get; set; }
        public double Ixy { get; set; }
        public double I11 { get; set; }
        public double I22 { get; set; }
        public double J { get; set; }
        public double ShearL1 { get; set; }
        public double ShearL2 { get; set; }
        public double ShearA1 { get; set; }
        public double ShearA2 { get; set; }
        public double Angle { get; set; }
        public Point2d Centroid { get; set; }

        public List<Brep> Breps { get; set; }

        public FrameSectionModel(string name, SectionGeometryTypes fptype, Types type, OffsetTypes offset = OffsetTypes.CC, double dimension1 = 0.0, double dimension2 = 0.0,
            double dimension3 = 0.0, double dimension4 = 0.0, double dimension5 = 0.0, double dimension6 = 0.0, double dimension7 = 0.0,
            double dimension8 = 0.0, double dimension9 = 0.0, double dimension10 = 0.0, double composite1 = 0.0, double composite2 = 0.0,
            double composite3 = 0.0, double composite4 = 0.0, double composite5 = 0.0)
            : base(name)
        {
            SectionGeometryType = fptype;
            Type = type;
            Offset = offset;
            Dimension1 = dimension1;
            Dimension2 = dimension2;
            Dimension3 = dimension3;
            Dimension4 = dimension4;
            Dimension5 = dimension5;
            Dimension6 = dimension6;
            Dimension7 = dimension7;
            Dimension8 = dimension8;
            Dimension9 = dimension9;
            Dimension10 = dimension10;
            CompositeData1 = composite1;
            CompositeData2 = composite2;
            CompositeData3 = composite3;
            CompositeData4 = composite4;
            CompositeData5 = composite5;
            Breps = new List<Brep>();
            CreateShapesAndCalculate();
        }

        public FrameSectionModel()
            : base()
        {
        }

        public FrameSectionModel(FrameSectionModel framePropertyModel)
            : base(framePropertyModel.Id, framePropertyModel.Name)
        {
            Type = framePropertyModel.Type;
            SectionGeometryType = framePropertyModel.SectionGeometryType;
            Offset = framePropertyModel.Offset;
            Dimension1 = framePropertyModel.Dimension1;
            Dimension2 = framePropertyModel.Dimension2;
            Dimension3 = framePropertyModel.Dimension3;
            Dimension4 = framePropertyModel.Dimension4;
            Dimension5 = framePropertyModel.Dimension5;
            Dimension6 = framePropertyModel.Dimension6;
            Dimension7 = framePropertyModel.Dimension7;
            Dimension8 = framePropertyModel.Dimension8;
            Dimension9 = framePropertyModel.Dimension9;
            Dimension10 = framePropertyModel.Dimension10;
            SectionArea = framePropertyModel.SectionArea;
            Ixx = framePropertyModel.Ixx;
            Iyy = framePropertyModel.Iyy;
            Ixy = framePropertyModel.Ixy;
            I11 = framePropertyModel.I11;
            I22 = framePropertyModel.I22;
            J = framePropertyModel.J;
            Angle = framePropertyModel.Angle;
            Centroid = framePropertyModel.Centroid;
            Breps = framePropertyModel.Breps;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as FrameSectionModel);
        }

        public bool Equals(FrameSectionModel other)
        {
            return !(other is null) &&
                   base.Equals(other) &&
                   Name == other.Name &&
                   SectionGeometryType == other.SectionGeometryType &&
                   Type == other.Type &&
                   Offset == other.Offset &&
                   Dimension1 == other.Dimension1 &&
                   Dimension2 == other.Dimension2 &&
                   Dimension3 == other.Dimension3 &&
                   Dimension4 == other.Dimension4 &&
                   Dimension5 == other.Dimension5 &&
                   Dimension6 == other.Dimension6 &&
                   Dimension7 == other.Dimension7 &&
                   Dimension8 == other.Dimension8 &&
                   Dimension9 == other.Dimension9 &&
                   Dimension10 == other.Dimension10;
        }

        public override int GetHashCode()
        {
            int hashCode = -23;
            hashCode = hashCode * -17 + base.GetHashCode();
            hashCode = hashCode * -17 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -17 + SectionGeometryType.GetHashCode();
            hashCode = hashCode * -17 + Offset.GetHashCode();
            hashCode = hashCode * -17 + Dimension1.GetHashCode();
            hashCode = hashCode * -17 + Dimension2.GetHashCode();
            hashCode = hashCode * -17 + Dimension3.GetHashCode();
            hashCode = hashCode * -17 + Dimension4.GetHashCode();
            hashCode = hashCode * -17 + Dimension5.GetHashCode();
            hashCode = hashCode * -17 + Dimension6.GetHashCode();
            hashCode = hashCode * -17 + Dimension7.GetHashCode();
            hashCode = hashCode * -17 + Dimension8.GetHashCode();
            hashCode = hashCode * -17 + Dimension9.GetHashCode();
            hashCode = hashCode * -17 + Dimension10.GetHashCode();
            hashCode = hashCode * -17 + Type.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(FrameSectionModel left, FrameSectionModel right)
        {
            return EqualityComparer<FrameSectionModel>.Default.Equals(left, right);
        }

        public static bool operator !=(FrameSectionModel left, FrameSectionModel right)
        {
            return !(left == right);
        }

        public void CreateShapesAndCalculate()
        {
            List<Brep> main_surfaces = new List<Brep>();
            double tol = 0.001;

            switch (Type)
            {
                case Types.DBUSER:
                    switch (SectionGeometryType)
                    {
                        case SectionGeometryTypes.SR:
                            {
                                ArcCurve border = new ArcCurve(new Circle(Dimension1 / 2));
                                Brep[] breps = Brep.CreatePlanarBreps(border, tol);
                                if (breps != null)
                                    main_surfaces.Add(breps[0]);
                            }
                            break;
                        case SectionGeometryTypes.P:
                            {
                                ArcCurve border = new ArcCurve(new Circle(Dimension1 / 2));
                                ArcCurve inner = new ArcCurve(new Circle(Dimension1 / 2 - Dimension2));
                                CurveList curves = new CurveList
                        {
                            border,
                            inner
                        };
                                Brep[] breps = Brep.CreatePlanarBreps(curves, tol);
                                if (breps != null)
                                    main_surfaces.Add(breps[0]);
                            }
                            break;
                        case SectionGeometryTypes.SB:
                            {
                                List<Point3d> pts = new List<Point3d>
                        {
                            new Point3d(-Dimension2 / 2, Dimension1 / 2, 0),
                            new Point3d(-Dimension2 / 2, -Dimension1 / 2, 0),
                            new Point3d(Dimension2 / 2, -Dimension1 / 2, 0),
                            new Point3d(Dimension2 / 2, Dimension1 / 2, 0),
                            new Point3d(-Dimension2 / 2, Dimension1 / 2, 0)
                        };
                                PolylineCurve border = new PolylineCurve(pts);
                                Brep[] breps = Brep.CreatePlanarBreps(border, tol);
                                if (breps != null)
                                    main_surfaces.Add(breps[0]);
                            }
                            break;
                        case SectionGeometryTypes.B:
                            {
                                List<Point3d> ptsOut = new List<Point3d>
                                {
                                    new Point3d(-Dimension2 / 2, -Dimension1 / 2, 0),
                                    new Point3d(Dimension2 / 2, -Dimension1 / 2, 0),
                                    new Point3d(Dimension2 / 2, Dimension1 / 2, 0),
                                    new Point3d(-Dimension2 / 2, Dimension1 / 2, 0),
                                    new Point3d(-Dimension2 / 2, -Dimension1 / 2, 0)
                                };
                                PolylineCurve border = new PolylineCurve(ptsOut);
                                List<Point3d> ptsIn = new List<Point3d>
                                {
                                    new Point3d(-Dimension2 / 2 + Dimension3, -Dimension1 / 2 + Dimension5, 0),
                                    new Point3d(Dimension2 / 2 - Dimension3, -Dimension1 / 2 + Dimension5, 0),
                                    new Point3d(Dimension2 / 2 - Dimension3, Dimension1 / 2 - Dimension4, 0),
                                    new Point3d(-Dimension2 / 2 + Dimension3, Dimension1 / 2 - Dimension4, 0),
                                    new Point3d(-Dimension2 / 2 + Dimension3, -Dimension1 / 2 + Dimension5, 0)
                                };
                                PolylineCurve inner = new PolylineCurve(ptsIn);

                                CurveList curves = new CurveList
                        {
                            border,
                            inner
                        };
                                Brep[] breps = Brep.CreatePlanarBreps(curves, tol);
                                if (breps != null)
                                    main_surfaces.Add(breps[0]);
                            }
                            break;
                        case SectionGeometryTypes.H:
                            {
                                List<Point3d> pts = new List<Point3d>
                        {
                            new Point3d(-Dimension5 / 2, 0, 0),
                            new Point3d(Dimension5 / 2, 0, 0),
                            new Point3d(Dimension5 / 2, Dimension6, 0),
                            new Point3d(Dimension3 / 2, Dimension6, 0),
                            new Point3d(Dimension3 / 2, Dimension1 - Dimension4, 0),
                            new Point3d(Dimension2 / 2, Dimension1 - Dimension4, 0),
                            new Point3d(Dimension2 / 2, Dimension1, 0),
                            new Point3d(-Dimension2 / 2, Dimension1, 0),
                            new Point3d(-Dimension2 / 2, Dimension1 - Dimension4, 0),
                            new Point3d(-Dimension3 / 2, Dimension1 - Dimension4, 0),
                            new Point3d(-Dimension3 / 2, Dimension6, 0),
                            new Point3d(-Dimension5 / 2, Dimension6, 0),
                            new Point3d(-Dimension5 / 2, 0, 0)
                        };
                                PolylineCurve border = new PolylineCurve(pts);
                                Brep[] breps = Brep.CreatePlanarBreps(border, tol);
                                if (breps != null)
                                    main_surfaces.Add(breps[0]);
                            }
                            break;
                        case SectionGeometryTypes.C:
                            {
                                List<Point3d> pts = new List<Point3d>
                                {
                                    new Point3d(0, 0, 0),
                                    new Point3d(Dimension5, 0, 0),
                                    new Point3d(Dimension5, Dimension6, 0),
                                    new Point3d(Dimension3, Dimension6, 0),
                                    new Point3d(Dimension3, Dimension1 - Dimension4, 0),
                                    new Point3d(Dimension2, Dimension1 - Dimension4, 0),
                                    new Point3d(Dimension2, Dimension1, 0),
                                    new Point3d(0, Dimension1, 0),
                                    new Point3d(0, 0, 0),
                                };
                                PolylineCurve border = new PolylineCurve(pts);
                                Brep[] breps = Brep.CreatePlanarBreps(border, tol);
                                if (breps != null)
                                    main_surfaces.Add(breps[0]);
                            }
                            break;
                        case SectionGeometryTypes.L:
                            {
                                List<Point3d> pts = new List<Point3d>
                                {
                                    new Point3d(0, 0, 0),
                                    new Point3d(Dimension2, 0, 0),
                                    new Point3d(Dimension2, Dimension4, 0),
                                    new Point3d(Dimension3, Dimension4, 0),
                                    new Point3d(Dimension3, Dimension1, 0),
                                    new Point3d(0, Dimension1, 0),
                                    new Point3d(0, 0, 0),
                                };
                                PolylineCurve border = new PolylineCurve(pts);
                                Brep[] breps = Brep.CreatePlanarBreps(border, tol);
                                if (breps != null)
                                    main_surfaces.Add(breps[0]);
                            }
                            break;
                            /*
                        case SectionModel.SectionTypes.T:
                            {
                                List<Point3d> pts = new List<Point3d>
                                {
                                    new Point3d(-_beamProperty.T2 / 2, -c.Y, 0),
                                    new Point3d(_beamProperty.T2 / 2, -c.Y, 0),
                                    new Point3d(_beamProperty.T2 / 2, -c.Y + _beamProperty.D - _beamProperty.T1, 0),
                                    new Point3d(_beamProperty.B / 2 - _beamProperty.T3, -c.Y + _beamProperty.D - _beamProperty.T1, 0)
                                };
                                if (_beamProperty.T3 > 0 && _beamProperty.L > 0)
                                {
                                    pts.Add(new Point3d(_beamProperty.B / 2 - _beamProperty.T3, -c.Y + _beamProperty.D - _beamProperty.T1 - _beamProperty.L, 0));
                                    pts.Add(new Point3d(_beamProperty.B / 2, -c.Y + _beamProperty.D - _beamProperty.T1 - _beamProperty.L, 0));
                                }
                                pts.Add(new Point3d(_beamProperty.B / 2, -c.Y + _beamProperty.D, 0));
                                pts.Add(new Point3d(-_beamProperty.B / 2, -c.Y + _beamProperty.D, 0));
                                pts.Add(new Point3d(-_beamProperty.B / 2, -c.Y + _beamProperty.D - _beamProperty.T1 - _beamProperty.L, 0));
                                if (_beamProperty.T3 > 0 && _beamProperty.L > 0)
                                {
                                    pts.Add(new Point3d(-_beamProperty.B / 2 + _beamProperty.T3, -c.Y + _beamProperty.D - _beamProperty.T1 - _beamProperty.L, 0));
                                    pts.Add(new Point3d(-_beamProperty.B / 2 + _beamProperty.T3, -c.Y + _beamProperty.D - _beamProperty.T1, 0));
                                }
                                pts.Add(new Point3d(-_beamProperty.T2 / 2, -c.Y + _beamProperty.D - _beamProperty.T1, 0));
                                pts.Add(new Point3d(-_beamProperty.T2 / 2, -c.Y, 0));
                                PolylineCurve border = new PolylineCurve(pts);
                                Brep[] breps = Brep.CreatePlanarBreps(border, tol);
                                if (breps != null)
                                    main_surfaces.Add(breps[0]);
                            }
                            break;
                        case SectionModel.SectionTypes.C:
                        case SectionModel.SectionTypes.Omega:
                            {
                                List<Point3d> pts = new List<Point3d> { new Point3d(-c.X, -c.Y, 0) };
                                pts.Add(Point3d.Add(pts[0], new Vector3d(0, _beamProperty.D, 0)));
                                pts.Add(Point3d.Add(pts[1], new Vector3d(_beamProperty.B, 0, 0)));
                                pts.Add(Point3d.Add(pts[2], new Vector3d(0, -_beamProperty.T1, 0)));
                                pts.Add(Point3d.Add(pts[3], new Vector3d(-_beamProperty.B + _beamProperty.T2, 0, 0)));
                                pts.Add(Point3d.Add(pts[4], new Vector3d(0, -_beamProperty.D + _beamProperty.T1 * 2, 0)));
                                pts.Add(Point3d.Add(pts[5], new Vector3d(_beamProperty.B - _beamProperty.T2, 0, 0)));
                                pts.Add(Point3d.Add(pts[6], new Vector3d(0, -_beamProperty.T1, 0)));
                                pts.Add(pts[0]);
                                PolylineCurve border = new PolylineCurve(pts);
                                Brep[] breps = Brep.CreatePlanarBreps(border, tol);
                                if (breps != null)
                                    main_surfaces.Add(breps[0]);
                            }
                            break;
                        case SectionModel.SectionTypes.Angle:
                            {
                                List<Point3d> pts = new List<Point3d> { new Point3d(-c.X, -c.Y, 0) };
                                pts.Add(Point3d.Add(pts[0], new Vector3d(0, _beamProperty.D, 0)));
                                pts.Add(Point3d.Add(pts[1], new Vector3d(_beamProperty.T2, 0, 0)));
                                pts.Add(Point3d.Add(pts[2], new Vector3d(0, -_beamProperty.D + _beamProperty.T1, 0)));
                                pts.Add(Point3d.Add(pts[3], new Vector3d(_beamProperty.B - _beamProperty.T2, 0, 0)));
                                pts.Add(Point3d.Add(pts[4], new Vector3d(0, -_beamProperty.T1, 0)));
                                pts.Add(Point3d.Add(pts[5], new Vector3d(-_beamProperty.B, 0, 0)));
                                pts.Add(pts[0]);
                                PolylineCurve border = new PolylineCurve(pts);
                                Brep[] breps = Brep.CreatePlanarBreps(border, tol);
                                if (breps != null)
                                    main_surfaces.Add(breps[0]);
                            }
                            break;
                        case SectionModel.SectionTypes.Z:
                            {

                            }
                            break;
                        case SectionModel.SectionTypes.Generic:
                            {
                                //equivalent circular section
                                ArcCurve border = new ArcCurve(new Circle(0.001 * System.Math.Sqrt(_beamProperty.SectionArea / System.Math.PI)));
                                Brep[] breps = Brep.CreatePlanarBreps(border, tol);
                                if (breps != null)
                                    main_surfaces.Add(breps[0]);
                            }
                            break;
                        case SectionModel.SectionTypes.GenericShapes:
                            {
                                foreach (Maffeis.Geometry.Shape2d s in _beamProperty.GenericShapes)
                                {
                                    CurveList curves = new CurveList();
                                    //fill
                                    List<Point3d> pts;
                                    pts = new List<Point3d>();
                                    foreach (Maffeis.Geometry.Point2d p in s.Fill)
                                    {
                                        pts.Add(new Point3d(p.X - c.X, p.Y - c.Y, 0));
                                    }
                                    pts.Add(new Point3d(s.Fill[0].X - c.X, s.Fill[0].Y - c.Y, 0));
                                    curves.Add(new PolylineCurve(pts));
                                    if (s.Holes != null)
                                    {
                                        for (int i = 0; i < s.Holes2d.Length; i++)
                                        {
                                            Maffeis.Geometry.Polygon2d hole = s.Holes2d[i];
                                            pts = new List<Point3d>();
                                            foreach (Maffeis.Geometry.Point2d p in hole)
                                            {
                                                pts.Add(new Point3d(p.X - c.X, p.Y - c.Y, 0));
                                            }
                                            pts.Add(new Point3d(hole[0].X - c.X, hole[0].Y - c.Y, 0));
                                            curves.Add(new PolylineCurve(pts));
                                        }
                                    }
                                    main_surfaces.Add(Brep.CreatePlanarBreps(curves, tol)[0]);
                                }
                                break;
                            }
                            */
                    }
                    break;
                case Types.COMPOSITE_I:
                    {
                        List<Point3d> pts = new List<Point3d>
                        {
                            new Point3d(-Dimension5 / 2, 0, 0),
                            new Point3d(Dimension5 / 2, 0, 0),
                            new Point3d(Dimension5 / 2, Dimension6, 0),
                            new Point3d(Dimension2 / 2, Dimension6, 0),
                            new Point3d(Dimension2 / 2, Dimension1 - Dimension4, 0),
                            new Point3d(Dimension3 / 2, Dimension1 - Dimension4, 0),
                            new Point3d(Dimension3 / 2, Dimension1, 0),
                            new Point3d(-Dimension3 / 2, Dimension1, 0),
                            new Point3d(-Dimension3 / 2, Dimension1 - Dimension4, 0),
                            new Point3d(-Dimension2 / 2, Dimension1 - Dimension4, 0),
                            new Point3d(-Dimension2 / 2, Dimension6, 0),
                            new Point3d(-Dimension5 / 2, Dimension6, 0),
                            new Point3d(-Dimension5 / 2, 0, 0)
                        };
                        PolylineCurve border = new PolylineCurve(pts);
                        Brep[] breps = Brep.CreatePlanarBreps(border, tol);
                        if (breps != null)
                            main_surfaces.Add(breps[0]);

                        List<Point3d> ptsRect = new List<Point3d>
                        {
                            new Point3d(-Dimension7 / 2, Dimension8 / 2, 0),
                            new Point3d(-Dimension7 / 2, -Dimension8 / 2, 0),
                            new Point3d(Dimension7 / 2, -Dimension8 / 2, 0),
                            new Point3d(Dimension7 / 2, Dimension8 / 2, 0),
                            new Point3d(-Dimension7 / 2, Dimension8 / 2, 0)
                        };
                        PolylineCurve borderSlab = new PolylineCurve(ptsRect);
                        Brep[] breps2 = Brep.CreatePlanarBreps(borderSlab, tol);
                        if (breps2 != null)
                        {
                            Brep bb = breps2[0];
                            bb.Translate(0, Dimension1, 0);
                            bb.Translate(0, Dimension8 / 2, 0);
                            bb.Translate(0, Dimension9, 0);
                            main_surfaces.Add(breps2[0]);
                        }

                        break;
                    }
            }
            switch (Type)
            {
                case Types.DBUSER:
                    switch (SectionGeometryType)
                    {
                        case SectionGeometryTypes.SR:
                        case SectionGeometryTypes.P:
                        case SectionGeometryTypes.SB:
                        case SectionGeometryTypes.B:
                        case SectionGeometryTypes.L:
                        case SectionGeometryTypes.C:
                        case SectionGeometryTypes.H:
                            {
                                if (main_surfaces.Count > 0)
                                {
                                    Brep brep = main_surfaces[0];

                                    AreaMassProperties prop = AreaMassProperties.Compute(brep, true, true, true, true);
                                    prop.CentroidCoordinatesPrincipalMomentsOfInertia(out double x, out Vector3d xv, out double y, out Vector3d yv, out double z, out Vector3d zv);

                                    SectionArea = prop.Area;
                                    Centroid = new Point2d(prop.Centroid.X, prop.Centroid.Y);
                                    Ixx = prop.CentroidCoordinatesMomentsOfInertia.X;
                                    Iyy = prop.CentroidCoordinatesMomentsOfInertia.Y;
                                    I11 = prop.CentroidCoordinatesMomentsOfInertia.X;
                                    I22 = prop.CentroidCoordinatesMomentsOfInertia.Y;
                                    Ixy = prop.CentroidCoordinatesMomentsOfInertia.Z;
                                    Angle = Vector3d.VectorAngle(xv, Vector3d.XAxis);
                                    if (Angle >= Math.PI)
                                        Angle -= Math.PI;
                                    if (Angle < 0)
                                        Angle += Math.PI;
                                    brep.Translate(-Centroid.X, -Centroid.Y, 0);

                                    Breps = new List<Brep> { brep };
                                }
                            }
                            break;
                    }
                    break;
                case Types.COMPOSITE_I:
                    {
                        if (main_surfaces.Count > 0)
                        {
                            Brep steel = main_surfaces[0].DuplicateBrep();

                            AreaMassProperties propSteel = AreaMassProperties.Compute(steel, true, true, true, true);
                            propSteel.CentroidCoordinatesPrincipalMomentsOfInertia(out double x, out Vector3d xv, out double y, out Vector3d yv, out double z, out Vector3d zv);

                            Brep concrete = main_surfaces[1].DuplicateBrep();
                            Point3d concrCentr = concrete.GetBoundingBox(true).Center;

                            var concreteScaled = concrete.DuplicateBrep();
                            //concreteScaled.Scale(1 / CompositeData1);
                            concreteScaled.Translate(concrCentr - concreteScaled.GetBoundingBox(true).Center);

                            AreaMassProperties propConcre = AreaMassProperties.Compute(concrete, true, true, true, true);
                            propConcre.CentroidCoordinatesPrincipalMomentsOfInertia(out double xC, out Vector3d xvC, out double yC, out Vector3d yvC, out double zC, out Vector3d zvC);

                            SectionArea = propSteel.Area + propConcre.Area / CompositeData1;

                            double xCentroid = (propSteel.Area * propSteel.Centroid.X + propConcre.Area * propConcre.Centroid.X / CompositeData1) /
                                (SectionArea);
                            double yCentroid = (propSteel.Area * propSteel.Centroid.Y + propConcre.Area * propConcre.Centroid.Y / CompositeData1) /
                                (SectionArea);

                            Centroid = new Point2d(xCentroid, yCentroid);

                            double Jxx = (propSteel.Area * propSteel.CentroidCoordinatesMomentsOfInertia.X + propConcre.Area * propConcre.CentroidCoordinatesMomentsOfInertia.X / CompositeData1) /
                                (propSteel.Area + propConcre.Area);
                            double Jyy = (propSteel.Area * propSteel.CentroidCoordinatesMomentsOfInertia.Y + propConcre.Area * propConcre.CentroidCoordinatesMomentsOfInertia.Y / CompositeData1) /
                                (propSteel.Area + propConcre.Area);

                            Angle = 0;
                            //var vsteel = new Vector3d(propSteel.Centroid.X - Centroid.X, propSteel.Centroid.Y - Centroid.Y, 0);
                            //var vConcrete = new Vector3d(propConcre.Centroid.X - Centroid.X, propConcre.Centroid.Y - Centroid.Y, 0);
                            steel.Translate(-Centroid.X, -Centroid.Y, 0);
                            concrete.Translate(-Centroid.X, -Centroid.Y, 0);

                            Breps = new List<Brep> { steel, concrete };
                        }
                    }
                    break;
            }
        }
    }
}
