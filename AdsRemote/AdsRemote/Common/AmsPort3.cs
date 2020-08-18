namespace AdsRemote.Common
{
    /// <summary>
    /// TwinCAT 3 ADS Ports
    /// </summary>
    public enum AmsPort3
    {
        Router = 1,
        Debugger = 2,
        Logger = 100,
        EventLog = 110,
        // ReSharper disable once UnusedMember.Global
        R0Realtime = 200,
        // ReSharper disable once UnusedMember.Global
        R0Trace = 290,
        // ReSharper disable once UnusedMember.Global
        R0Io = 350,
        // ReSharper disable once UnusedMember.Global
        R0AdditionalTask1 = 350,
        // ReSharper disable once UnusedMember.Global
        R0AdditionalTask2 = 351,
        // ReSharper disable once UnusedMember.Global
        R0Nc = 500,
        // ReSharper disable once UnusedMember.Global
        R0Ncsaf = 501,
        // ReSharper disable once UnusedMember.Global
        R0Ncsvb = 511,
        // ReSharper disable once UnusedMember.Global
        R0Isg = 550,
        // ReSharper disable once UnusedMember.Global
        R0Cnc = 600,
        // ReSharper disable once UnusedMember.Global
        R0Line = 700,
        // ReSharper disable once UnusedMember.Global
        R0Plc = 800,
        PlcRuntime1 = 851,
        PlcRuntime2 = 852,
        PlcRuntime3 = 853,
        PlcRuntime4 = 854,
        // ReSharper disable once UnusedMember.Global
        CamshaftController = 900,
        // ReSharper disable once UnusedMember.Global
        R0Camtool = 950,
        // ReSharper disable once UnusedMember.Global
        R0User = 2000,
        // ReSharper disable once UnusedMember.Global
        R3Ctrlprog = 10000,
        SystemService = 10000,
        // ReSharper disable once UnusedMember.Global
        R3Sysctrl = 10001,
        // ReSharper disable once UnusedMember.Global
        R3Syssampler = 10100,
        // ReSharper disable once UnusedMember.Global
        R3Tcprawconn = 10200,
        // ReSharper disable once UnusedMember.Global
        R3Tcpipserver = 10201,
        // ReSharper disable once UnusedMember.Global
        R3Sysmanager = 10300,
        // ReSharper disable once UnusedMember.Global
        R3Smsserver = 10400,
        // ReSharper disable once UnusedMember.Global
        R3Modbusserver = 10500,
        // ReSharper disable once UnusedMember.Global
        R3S7Server = 10600,
        // ReSharper disable once UnusedMember.Global
        R3Plccontrol = 10800,
        // ReSharper disable once UnusedMember.Global
        R3Ncctrl = 11000,
        // ReSharper disable once UnusedMember.Global
        R3Ncinterpreter = 11500,
        // ReSharper disable once UnusedMember.Global
        R3Streckectrl = 12000,
        // ReSharper disable once UnusedMember.Global
        R3Camctrl = 13000,
        // ReSharper disable once UnusedMember.Global
        R3Scope = 14000,
        // ReSharper disable once UnusedMember.Global
        R3Sinech1 = 15000,
        // ReSharper disable once UnusedMember.Global
        R3Controlnet = 16000,
        // ReSharper disable once UnusedMember.Global
        R3Opcserver = 17000,
        // ReSharper disable once UnusedMember.Global
        R3Opcclient = 17500,
        // ReSharper disable once UnusedMember.Global
        Usedefault = 65535
    }
}
