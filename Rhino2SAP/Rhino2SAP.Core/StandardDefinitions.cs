namespace Rhino2SAP.Core;

public static class StandardDefinitions
{
    public static Fragment Material(string name,int type,double young,double poisson,double alpha,double weightDensity)
    {
        if(young<=0||!double.IsFinite(young))throw new ArgumentOutOfRangeException(nameof(young));
        if(poisson<=-1||poisson>=.5)throw new ArgumentOutOfRangeException(nameof(poisson));
        if(weightDensity<0||!double.IsFinite(weightDensity))throw new ArgumentOutOfRangeException(nameof(weightDensity));
        return new Fragment([
            Operation.Create("PropMaterial.SetMaterial",("Name",name),("MatType",type)),
            Operation.Create("PropMaterial.SetMPIsotropic",("Name",name),("E",young),("U",poisson),("A",alpha)),
            Operation.Create("PropMaterial.SetWeightAndMass",("Name",name),("MyOption",1),("Value",weightDensity))]);
    }
    public static Fragment LoadPattern(string name,int type,double selfWeight=0)=>new([Operation.Create("LoadPatterns.Add",("Name",name),("MyType",type),("SelfWTMultiplier",selfWeight),("AddAnalysisCase",true))]);
    public static Fragment StaticCase(string name,IReadOnlyList<string> patterns,IReadOnlyList<double> factors,string solver="StaticLinear")
    {
        if(patterns.Count!=factors.Count||patterns.Count==0)throw new ArgumentException("One factor is required per load pattern.");
        if(solver is not ("StaticLinear" or "StaticNonlinear" or "Buckling"))throw new ArgumentException("Choose StaticLinear, StaticNonlinear, or Buckling.");
        return new([
            Operation.Create($"LoadCases.{solver}.SetCase",("Name",name)),
            Operation.Create($"LoadCases.{solver}.SetLoads",("Name",name),("NumberLoads",patterns.Count),("LoadType",patterns.Select(_=>"Load").ToArray()),("LoadName",patterns),("SF",factors))]);
    }
    public static Fragment Combination(string name,int type,IReadOnlyList<string> cases,IReadOnlyList<double> factors,IReadOnlyList<bool>? combinations=null)
    {
        if(cases.Count!=factors.Count||cases.Count==0||combinations!=null&&combinations.Count!=cases.Count)throw new ArgumentException("One factor and optional combination flag per entry are required.");
        var ops=new List<Operation>{Operation.Create("RespCombo.Add",("Name",name),("ComboType",type))};
        ops.AddRange(cases.Select((c,i)=>Operation.Create("RespCombo.SetCaseList",("Name",name),("CNameType",combinations?[i]==true?1:0),("CName",c),("SF",factors[i]))));return new(ops);
    }
    public static SapModel AxialExample()
    {
        var material=Material("S355",1,210e6,.3,1.2e-5,0);
        var section=new Fragment([Operation.Create("PropFrame.SetRectangle",("Name","R"),("MatProp","S355"),("T3",.2),("T2",.1))]);
        var geometry=new Fragment(elements:[new("BASE",ElementKind.Node,[new(0,0,0)]),new("TIP",ElementKind.Node,[new(3,0,0)]),new("F1",ElementKind.Frame,[new(0,0,0),new(3,0,0)],"R")]);
        var loading=new Fragment([Operation.Create("PointObj.SetRestraint",("Name","BASE"),("Value",new[]{true,true,true,true,true,true})),Operation.Create("PointObj.SetLoadForce",("Name","TIP"),("LoadPat","AXIAL"),("Value",new[]{1.0,0,0,0,0,0}),("Replace",false),("CSys","Global"))]);
        return new(Fragment.Combine([material,section,geometry,LoadPattern("AXIAL",8),loading]));
    }
}
