//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.IO;
//using Grasshopper.Kernel.Data;
//using Grasshopper.Kernel.Types;
//using Rhino;
//using Rhino.Geometry;
//using Rhino.Runtime;
//using Rhino2Midas.Core.Elements;
//using Rhino2Midas.Core.ElementProperties;

//namespace Rhino2Midas.Grasshopper.Helper
//{
//    public class HelperMethod
//    {
//        public static bool IsCorrectTypeTree(GH_Structure<IGH_Goo> listInGhGoo, string type)
//        {
//            for (int i = 0; i < listInGhGoo.DataCount; i++)
//            {
//                IGH_Goo val = listInGhGoo[listInGhGoo[0], i];
//                if (val == null || val.TypeName != type)
//                {
//                    return false;
//                }
//            }
//            return true;
//        }

//        public static bool IsCorrectTypeList(List<IGH_Goo> listInGhGoo, string type)
//        {
//            for (int i = 0; i < listInGhGoo.Count; i++)
//            {
//                IGH_Goo item = listInGhGoo[i];
//                if (item == null || item.TypeName != type)
//                {
//                    return false;
//                }
//            }
//            return true;
//        }

//        public static List<Line> ConvertFrameElementToLine(List<FrameElementModel> frameElementList)
//        {
//            List<Line> list = new List<Line>();
//            for (int i = 0; i < frameElementList.Count; i++)
//            {
//                FrameElementModel frameElement = frameElementList[i];
//                Point3d val = new Point3d(frameElement.NodeStart.X, frameElement.NodeStart.Y, frameElement.NodeStart.Z);
//                Point3d val2 = new Point3d(frameElement.NodeEnd.X, frameElement.NodeEnd.Y, frameElement.NodeEnd.Z);
//                list.Add(new Line(val, val2));
//            }
//            return list;
//        }

//        public static List<Brep> ConvertAreaElementToLine(List<AreaElementModel> areaElementList)
//        {
//            List<Brep> list = new List<Brep>();
//            for (int i = 0; i < areaElementList.Count; i++)
//            {
//                AreaElementModel areaElement = areaElementList[i];
//                if (areaElement.NodeList.Count == 3)
//                {
//                    Point3d val = new Point3d(areaElement.NodeList[0].X, areaElement.NodeList[0].Y, areaElement.NodeList[0].Z);
//                    Point3d val2 = new Point3d(areaElement.NodeList[1].X, areaElement.NodeList[1].Y, areaElement.NodeList[1].Z);
//                    Point3d val3 = new Point3d(areaElement.NodeList[2].X, areaElement.NodeList[2].Y, areaElement.NodeList[2].Z);
//                    Brep item = Brep.CreateFromCornerPoints(val, val2, val3, 0.001);
//                    list.Add(item);
//                }
//                else if (areaElement.NodeList.Count == 4)
//                {
//                    Point3d val4 = new Point3d(areaElement.NodeList[0].X, areaElement.NodeList[0].Y, areaElement.NodeList[0].Z);
//                    Point3d val5 = new Point3d(areaElement.NodeList[1].X, areaElement.NodeList[1].Y, areaElement.NodeList[1].Z);
//                    Point3d val6 = new Point3d(areaElement.NodeList[2].X, areaElement.NodeList[2].Y, areaElement.NodeList[2].Z);
//                    Point3d val7 = new Point3d(areaElement.NodeList[3].X, areaElement.NodeList[3].Y, areaElement.NodeList[3].Z);
//                    Brep item2 = Brep.CreateFromCornerPoints(val4, val5, val6, val7, 0.001);
//                    list.Add(item2);
//                }
//            }
//            return list;
//        }

//        public static (List<Brep>, List<PolylineCurve>) ExtrudeFrameElement(List<FrameElementModel> frameElementList)
//        {
//            List<Brep> list = new List<Brep>();
//            List<PolylineCurve> list2 = new List<PolylineCurve>();

//            foreach (FrameElementModel frameElement in frameElementList)
//            {
//                Point3d val1 = new Point3d(frameElement.NodeStart.X, frameElement.NodeStart.Y, frameElement.NodeStart.Z);
//                Point3d val2 = new Point3d(frameElement.NodeEnd.X, frameElement.NodeEnd.Y, frameElement.NodeEnd.Z);
//                Vector3d val3 = new Vector3d(val2.X - val1.X, val2.Y - val1.Y, val2.Z - val1.Z);
//                Vector3d val4 = new Vector3d(val2.X - val1.X, val2.Y - val1.Y, 0.0);
//                double angle = frameElement.Angle;
//                FramePropertyModel.FramePropertyTypes type = frameElement.FrameProperty.Type;

//                switch (type)
//                {
//                    default:
//                        if (!(type == FramePropertyModel.FramePropertyTypes.SB))
//                        {
//                            if (type == FramePropertyModel.FramePropertyTypes.SR || type == FramePropertyModel.FramePropertyTypes.P)
//                            {
//                                double dimension = frameElement.FrameProperty.Dimension1;
//                                Line val5 = new Line(val1, val2);
//                                Curve val6 = val5.ToNurbsCurve();
//                                Brep[] array = Brep.CreatePipe(val6, dimension * 0.5, false, (PipeCapMode)0, true, 0.001, 0.001);
//                                list.Add(array[0]);
//                            }
//                            break;
//                        }
//                        goto case FramePropertyModel.FramePropertyTypes.L;
//                    case FramePropertyModel.FramePropertyTypes.L:
//                    case FramePropertyModel.FramePropertyTypes.C:
//                    case FramePropertyModel.FramePropertyTypes.H:
//                    case FramePropertyModel.FramePropertyTypes.T:
//                    case FramePropertyModel.FramePropertyTypes.B:
//                        {
//                            PolylineCurve val7 = new PolylineCurve();
//                            switch (type)
//                            {
//                                case FramePropertyModel.FramePropertyTypes.SB:
//                                    {
//                                        List<Point3d> list7 = new List<Point3d>();
//                                        FramePropertyModel.OffsetTypes offset5 = frameElement.FrameProperty.Offset;
//                                        double dimension23 = frameElement.FrameProperty.Dimension1;
//                                        double dimension24 = frameElement.FrameProperty.Dimension2;
//                                        Point3d item53 = new Point3d(-1.0 * dimension24 * 0.5, dimension23 * 0.5, 0.0);
//                                        list7.Add(item53);
//                                        Point3d item54 = new Point3d(dimension24 * 0.5, dimension23 * 0.5, 0.0);
//                                        list7.Add(item54);
//                                        Point3d item55 = new Point3d(dimension24 * 0.5, -1.0 * dimension23 * 0.5, 0.0);
//                                        list7.Add(item55);
//                                        Point3d item56 = new Point3d(-1.0 * dimension24 * 0.5, -1.0 * dimension23 * 0.5, 0.0);
//                                        list7.Add(item56);
//                                        list7.Add(item53);
//                                        val7 = new PolylineCurve((IEnumerable<Point3d>)list7);
//                                        MoveProfileOffset(val7, offset5, dimension23, dimension24);
//                                        break;
//                                    }
//                                case FramePropertyModel.FramePropertyTypes.B:
//                                    {
//                                        List<Point3d> list6 = new List<Point3d>();
//                                        var offset4 = frameElement.FrameProperty.Offset;
//                                        double dimension18 = frameElement.FrameProperty.Dimension1;
//                                        double dimension19 = frameElement.FrameProperty.Dimension2;
//                                        double dimension20 = frameElement.FrameProperty.Dimension3;
//                                        double dimension21 = frameElement.FrameProperty.Dimension4;
//                                        double num3 = frameElement.FrameProperty.Dimension5;
//                                        double dimension22 = frameElement.FrameProperty.Dimension6;
//                                        if (dimension19 - dimension20 >= num3)
//                                        {
//                                            num3 = dimension19 - dimension20;
//                                        }
//                                        if (dimension19 - dimension20 == num3)
//                                        {
//                                            Point3d item37 = new Point3d(-1.0 * dimension19 * 0.5, dimension18 * 0.5, 0.0);
//                                            list6.Add(item37);
//                                            Point3d item38 = new Point3d(dimension19 * 0.5, dimension18 * 0.5, 0.0);
//                                            list6.Add(item38);
//                                            Point3d item39 = new Point3d(dimension19 * 0.5, -1.0 * dimension18 * 0.5, 0.0);
//                                            list6.Add(item39);
//                                            Point3d item40 = new Point3d(-1.0 * dimension19 * 0.5, -1.0 * dimension18 * 0.5, 0.0);
//                                            list6.Add(item40);
//                                            list6.Add(item37);
//                                        }
//                                        else
//                                        {
//                                            Point3d item41 = new Point3d(-1.0 * dimension19 * 0.5, dimension18 * 0.5, 0.0);
//                                            list6.Add(item41);
//                                            Point3d item42 = new Point3d(dimension19 * 0.5, dimension18 * 0.5, 0.0);
//                                            list6.Add(item42);
//                                            Point3d item43 = new Point3d(dimension19 * 0.5, dimension18 * 0.5 - dimension21, 0.0);
//                                            list6.Add(item43);
//                                            Point3d item44 = new Point3d(num3 * 0.5 + dimension20 * 0.5, dimension18 * 0.5 - dimension21, 0.0);
//                                            list6.Add(item44);
//                                            Point3d item45 = new Point3d(num3 * 0.5 + dimension20 * 0.5, -1.0 * dimension18 * 0.5 + dimension22, 0.0);
//                                            list6.Add(item45);
//                                            Point3d item46 = new Point3d(dimension19 * 0.5, -1.0 * dimension18 * 0.5 + dimension22, 0.0);
//                                            list6.Add(item46);
//                                            Point3d item47 = new Point3d(dimension19 * 0.5, -1.0 * dimension18 * 0.5, 0.0);
//                                            list6.Add(item47);
//                                            Point3d item48 = new Point3d(-1.0 * dimension19 * 0.5, -1.0 * dimension18 * 0.5, 0.0);
//                                            list6.Add(item48);
//                                            Point3d item49 = new Point3d(-1.0 * dimension19 * 0.5, -1.0 * dimension18 * 0.5 + dimension22, 0.0);
//                                            list6.Add(item49);
//                                            Point3d item50 = new Point3d(-1.0 * num3 * 0.5 - dimension20 * 0.5, -1.0 * dimension18 * 0.5 + dimension22, 0.0);
//                                            list6.Add(item50);
//                                            Point3d item51 = new Point3d(-1.0 * num3 * 0.5 - dimension20 * 0.5, dimension18 * 0.5 - dimension21, 0.0);
//                                            list6.Add(item51);
//                                            Point3d item52 = new Point3d(-1.0 * dimension19 * 0.5, dimension18 * 0.5 - dimension21, 0.0);
//                                            list6.Add(item52);
//                                            list6.Add(item41);
//                                        }
//                                        val7 = new PolylineCurve((IEnumerable<Point3d>)list6);
//                                        MoveProfileOffset(val7, offset4, dimension18, dimension19);
//                                        break;
//                                    }
//                                case FramePropertyModel.FramePropertyTypes.L:
//                                    {
//                                        List<Point3d> list8 = new List<Point3d>();
//                                        var offset6 = frameElement.FrameProperty.Offset;
//                                        double dimension25 = frameElement.FrameProperty.Dimension1;
//                                        double dimension26 = frameElement.FrameProperty.Dimension2;
//                                        double dimension27 = frameElement.FrameProperty.Dimension3;
//                                        double dimension28 = frameElement.FrameProperty.Dimension4;
//                                        Point3d item57 = new Point3d(-1.0 * dimension26 * 0.5, dimension25 * 0.5, 0.0);
//                                        list8.Add(item57);
//                                        Point3d item58 = new Point3d(dimension26 * 0.5, dimension25 * 0.5, 0.0);
//                                        list8.Add(item58);
//                                        Point3d item59 = new Point3d(dimension26 * 0.5, -1.0 * dimension25 * 0.5, 0.0);
//                                        list8.Add(item59);
//                                        Point3d item60 = new Point3d(dimension26 * 0.5 - dimension27, -1.0 * dimension25 * 0.5, 0.0);
//                                        list8.Add(item60);
//                                        Point3d item61 = new Point3d(dimension26 * 0.5 - dimension27, dimension25 * 0.5 - dimension28, 0.0);
//                                        list8.Add(item61);
//                                        Point3d item62 = new Point3d(-1.0 * dimension26 * 0.5, dimension25 * 0.5 - dimension28, 0.0);
//                                        list8.Add(item62);
//                                        list8.Add(item57);
//                                        val7 = new PolylineCurve((IEnumerable<Point3d>)list8);
//                                        MoveProfileOffset(val7, offset6, dimension25, dimension26);
//                                        break;
//                                    }
//                                case FramePropertyModel.FramePropertyTypes.C:
//                                    {
//                                        List<Point3d> list5 = new List<Point3d>();
//                                        var offset3 = frameElement.FrameProperty.Offset;
//                                        double dimension12 = frameElement.FrameProperty.Dimension1;
//                                        double dimension13 = frameElement.FrameProperty.Dimension2;
//                                        double dimension14 = frameElement.FrameProperty.Dimension3;
//                                        double dimension15 = frameElement.FrameProperty.Dimension4;
//                                        double dimension16 = frameElement.FrameProperty.Dimension5;
//                                        double dimension17 = frameElement.FrameProperty.Dimension6;
//                                        double num = Math.Max(dimension13, dimension16);
//                                        double num2 = Math.Abs(dimension13 - dimension16);
//                                        if (dimension13 <= dimension16)
//                                        {
//                                            Point3d item21 = new Point3d(-1.0 * num * 0.5 + num2, dimension12 * 0.5, 0.0);
//                                            list5.Add(item21);
//                                            Point3d item22 = new Point3d(num * 0.5, dimension12 * 0.5, 0.0);
//                                            list5.Add(item22);
//                                            Point3d item23 = new Point3d(num * 0.5, -1.0 * dimension12 * 0.5, 0.0);
//                                            list5.Add(item23);
//                                            Point3d item24 = new Point3d(-1.0 * num * 0.5, -1.0 * dimension12 * 0.5, 0.0);
//                                            list5.Add(item24);
//                                            Point3d item25 = new Point3d(-1.0 * num * 0.5, -1.0 * dimension12 * 0.5 + dimension17, 0.0);
//                                            list5.Add(item25);
//                                            Point3d item26 = new Point3d(num * 0.5 - dimension14, -1.0 * dimension12 * 0.5 + dimension17, 0.0);
//                                            list5.Add(item26);
//                                            Point3d item27 = new Point3d(num * 0.5 - dimension14, dimension12 * 0.5 - dimension15, 0.0);
//                                            list5.Add(item27);
//                                            Point3d item28 = new Point3d(-1.0 * num * 0.5 + num2, dimension12 * 0.5 - dimension15, 0.0);
//                                            list5.Add(item28);
//                                            list5.Add(item21);
//                                        }
//                                        else
//                                        {
//                                            Point3d item29 = new Point3d(-1.0 * num * 0.5, dimension12 * 0.5, 0.0);
//                                            list5.Add(item29);
//                                            Point3d item30 = new Point3d(num * 0.5, dimension12 * 0.5, 0.0);
//                                            list5.Add(item30);
//                                            Point3d item31 = new Point3d(num * 0.5, -1.0 * dimension12 * 0.5, 0.0);
//                                            list5.Add(item31);
//                                            Point3d item32 = new Point3d(-1.0 * num * 0.5 + num2, -1.0 * dimension12 * 0.5, 0.0);
//                                            list5.Add(item32);
//                                            Point3d item33 = new Point3d(-1.0 * num * 0.5 + num2, -1.0 * dimension12 * 0.5 + dimension17, 0.0);
//                                            list5.Add(item33);
//                                            Point3d item34 = new Point3d(num * 0.5 - dimension14, -1.0 * dimension12 * 0.5 + dimension17, 0.0);
//                                            list5.Add(item34);
//                                            Point3d item35 = new Point3d(num * 0.5 - dimension14, dimension12 * 0.5 - dimension15, 0.0);
//                                            list5.Add(item35);
//                                            Point3d item36 = new Point3d(-1.0 * num * 0.5, dimension12 * 0.5 - dimension15, 0.0);
//                                            list5.Add(item36);
//                                            list5.Add(item29);
//                                        }
//                                        val7 = new PolylineCurve((IEnumerable<Point3d>)list5);
//                                        MoveProfileOffset(val7, offset3, dimension12, num);
//                                        break;
//                                    }
//                                case FramePropertyModel.FramePropertyTypes.H:
//                                    {
//                                        List<Point3d> list4 = new List<Point3d>();
//                                        var offset2 = frameElement.FrameProperty.Offset;
//                                        double dimension6 = frameElement.FrameProperty.Dimension1;
//                                        double dimension7 = frameElement.FrameProperty.Dimension2;
//                                        double dimension8 = frameElement.FrameProperty.Dimension3;
//                                        double dimension9 = frameElement.FrameProperty.Dimension4;
//                                        double dimension10 = frameElement.FrameProperty.Dimension5;
//                                        double dimension11 = frameElement.FrameProperty.Dimension6;
//                                        double width = Math.Max(dimension7, dimension10);
//                                        Point3d item9 = new Point3d(-1.0 * dimension7 * 0.5, dimension6 * 0.5, 0.0);
//                                        list4.Add(item9);
//                                        Point3d item10 = new Point3d(dimension7 * 0.5, dimension6 * 0.5, 0.0);
//                                        list4.Add(item10);
//                                        Point3d item11 = new Point3d(dimension7 * 0.5, dimension6 * 0.5 - dimension9, 0.0);
//                                        list4.Add(item11);
//                                        Point3d item12 = new Point3d(dimension8 * 0.5, dimension6 * 0.5 - dimension9, 0.0);
//                                        list4.Add(item12);
//                                        Point3d item13 = new Point3d(dimension8 * 0.5, -1.0 * dimension6 * 0.5 + dimension11, 0.0);
//                                        list4.Add(item13);
//                                        Point3d item14 = new Point3d(dimension10 * 0.5, -1.0 * dimension6 * 0.5 + dimension11, 0.0);
//                                        list4.Add(item14);
//                                        Point3d item15 = new Point3d(dimension10 * 0.5, -1.0 * dimension6 * 0.5, 0.0);
//                                        list4.Add(item15);
//                                        Point3d item16 = new Point3d(-1.0 * dimension10 * 0.5, -1.0 * dimension6 * 0.5, 0.0);
//                                        list4.Add(item16);
//                                        Point3d item17 = new Point3d(-1.0 * dimension10 * 0.5, -1.0 * dimension6 * 0.5 + dimension11, 0.0);
//                                        list4.Add(item17);
//                                        Point3d item18 = new Point3d(-1.0 * dimension8 * 0.5, -1.0 * dimension6 * 0.5 + dimension11, 0.0);
//                                        list4.Add(item18);
//                                        Point3d item19 = new Point3d(-1.0 * dimension8 * 0.5, dimension6 * 0.5 - dimension9, 0.0);
//                                        list4.Add(item19);
//                                        Point3d item20 = new Point3d(-1.0 * dimension7 * 0.5, dimension6 * 0.5 - dimension9, 0.0);
//                                        list4.Add(item20);
//                                        list4.Add(item9);
//                                        val7 = new PolylineCurve((IEnumerable<Point3d>)list4);
//                                        MoveProfileOffset(val7, offset2, dimension6, width);
//                                        break;
//                                    }
//                                case FramePropertyModel.FramePropertyTypes.T:
//                                    {
//                                        List<Point3d> list3 = new List<Point3d>();
//                                        var offset = frameElement.FrameProperty.Offset;
//                                        double dimension2 = frameElement.FrameProperty.Dimension1;
//                                        double dimension3 = frameElement.FrameProperty.Dimension2;
//                                        double dimension4 = frameElement.FrameProperty.Dimension3;
//                                        double dimension5 = frameElement.FrameProperty.Dimension4;
//                                        Point3d item = new Point3d(-1.0 * dimension3 * 0.5, dimension2 * 0.5, 0.0);
//                                        list3.Add(item);
//                                        Point3d item2 = new Point3d(dimension3 * 0.5, dimension2 * 0.5, 0.0);
//                                        list3.Add(item2);
//                                        Point3d item3 = new Point3d(dimension3 * 0.5, dimension2 * 0.5 - dimension5, 0.0);
//                                        list3.Add(item3);
//                                        Point3d item4 = new Point3d(dimension4 * 0.5, dimension2 * 0.5 - dimension5, 0.0);
//                                        list3.Add(item4);
//                                        Point3d item5 = new Point3d(dimension4 * 0.5, -1.0 * dimension2 * 0.5, 0.0);
//                                        list3.Add(item5);
//                                        Point3d item6 = new Point3d(-1.0 * dimension4 * 0.5, -1.0 * dimension2 * 0.5, 0.0);
//                                        list3.Add(item6);
//                                        Point3d item7 = new Point3d(-1.0 * dimension4 * 0.5, dimension2 * 0.5 - dimension5, 0.0);
//                                        list3.Add(item7);
//                                        Point3d item8 = new Point3d(-1.0 * dimension3 * 0.5, dimension2 * 0.5 - dimension5, 0.0);
//                                        list3.Add(item8);
//                                        list3.Add(item);
//                                        val7 = new PolylineCurve((IEnumerable<Point3d>)list3);
//                                        MoveProfileOffset(val7, offset, dimension2, dimension3);
//                                        break;
//                                    }
//                            }
//                            if (val7.IsValid)
//                            {
//                                if (val4.X != 0.0 || val4.Y != 0.0 || val4.Z != 0.0)
//                                {
//                                    val7.Transform(Transform.Rotation(Math.PI, new Point3d(0.0, 0.0, 0.0)));
//                                }
//                                val7.Transform(Transform.Rotation(-Math.PI / 2.0, new Point3d(0.0, 0.0, 0.0)));
//                                val7.Transform(Transform.Rotation(new Vector3d(1.0, 0.0, 0.0), val4, new Point3d(0.0, 0.0, 0.0)));
//                                val7.Transform(Transform.Rotation(new Vector3d(0.0, 0.0, 1.0), val3, new Point3d(0.0, 0.0, 0.0)));
//                                Vector3d val8 = new Vector3d(val1);
//                                val7.Transform(Transform.Translation(val8));
//                                val7.Transform(Transform.Rotation(RhinoMath.ToRadians(angle), val3, val1));
//                                val7.Transform(Transform.Rotation(RhinoMath.ToRadians(0.0), val3, val1));
//                                list2.Add(val7);
//                                Surface val9 = Surface.CreateExtrusion((Curve)(object)val7, val3);
//                                Brep item63 = val9.ToBrep();
//                                list.Add(item63);
//                            }
//                            break;
//                        }
//                }
//            }
//            return (list, list2);
//        }

//        public static void MoveProfileOffset(PolylineCurve profile, FramePropertyModel.OffsetTypes offset, double height, double width)
//        {            
//            double num = 0.0;
//            double num2 = 0.0;

//            switch (offset)
//            {
//                case FramePropertyModel.OffsetTypes.LT:
//                    num = width * 0.5;
//                    num2 = -1.0 * height * 0.5;
//                    break;
//                case FramePropertyModel.OffsetTypes.LC:
//                    num = width * 0.5;
//                    num2 = -1.0 * height * 0.5;
//                    break;
//                case FramePropertyModel.OffsetTypes.LB:
//                    num = width * 0.5;
//                    num2 = height * 0.5;
//                    break;
//                case FramePropertyModel.OffsetTypes.CT:
//                    num = 0.0;
//                    num2 = -1.0 * height * 0.5;
//                    break;
//                case FramePropertyModel.OffsetTypes.CC:
//                    num = 0.0;
//                    num2 = -1.0 * height * 0.5;
//                    break;
//                case FramePropertyModel.OffsetTypes.CB:
//                    num = 0.0;
//                    num2 = height * 0.5;
//                    break;
//                case FramePropertyModel.OffsetTypes.RT:
//                    num = -1.0 * width * 0.5;
//                    num2 = -1.0 * height * 0.5;
//                    break;
//                case FramePropertyModel.OffsetTypes.RC:
//                    num = -1.0 * width * 0.5;
//                    num2 = -1.0 * height * 0.5;
//                    break;
//                case FramePropertyModel.OffsetTypes.RB:
//                    num = -1.0 * width * 0.5;
//                    num2 = height * 0.5;
//                    break;
//                default:
//                    num = 0.0;
//                    num2 = 0.0;
//                    break;
//            }

//            profile.Transform(Transform.Translation(num, num2, 0.0));
//        }

//        public static List<Brep> ExtrudeAreaElement(List<Brep> areasAsBrep, List<AreaElementModel> areaElementList)
//        {
//            List<Brep> list = new List<Brep>();
//            int num = 0;
//            for (int i = 0; i < areasAsBrep.Count; i++)
//            {
//                Brep item2 = areasAsBrep[i];
//                Brep item = Brep.CreateFromOffsetFace(item2.Faces[0], areaElementList[num].AreaThickness.Thickness * 0.5, 0.001, true, true);
//                num++;
//                list.Add(item);
//            }
//            return list;
//        }

//        public static void WriteTextFile(List<string> textLineByLine, string filePath, string fileName)
//        {
//            string path = filePath + "\\" + fileName;
//            {
//                FileStream fileStream = new FileStream(path, FileMode.Create);
//                Encoding encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
//                StreamWriter streamWriter = new StreamWriter(fileStream, encoding);
//                for (int i = 0; i < textLineByLine.Count; i++)
//                {
//                    streamWriter.WriteLine(textLineByLine[i]);
//                }
//            }
//        }
//    }
//}
