namespace Rhino2SAP.Grasshopper;

// These definitions use the same SDK-driven sockets as all other native commands.
public sealed class TimeHistoryFunctionComponent : ApiCommandComponent
{
    protected override string MethodKey => "Func.FuncTH.SetUser";
    public TimeHistoryFunctionComponent() : base("Func.FuncTH.SetUser", "SAP Time History Function") { }
}

public sealed class SpectrumFunctionComponent : ApiCommandComponent
{
    protected override string MethodKey => "Func.FuncRS.SetUser";
    public SpectrumFunctionComponent() : base("Func.FuncRS.SetUser", "SAP Response Spectrum Function") { }
}

public sealed class SteadyStateFunctionComponent : ApiCommandComponent
{
    protected override string MethodKey => "Func.FuncSS.SetUser";
    public SteadyStateFunctionComponent() : base("Func.FuncSS.SetUser", "SAP Steady State Function") { }
}

public sealed class PowerSpectrumFunctionComponent : ApiCommandComponent
{
    protected override string MethodKey => "Func.FuncPSD.SetUser";
    public PowerSpectrumFunctionComponent() : base("Func.FuncPSD.SetUser", "SAP Power Spectrum Function") { }
}
