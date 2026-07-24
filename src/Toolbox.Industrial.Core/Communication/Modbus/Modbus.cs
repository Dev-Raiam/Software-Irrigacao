namespace Toolbox.Industrial.Core.Communication.Modbus;

public static class Modbus
{
    public static IModbusRTU RtuMaster(Configuration config, string loggerInfo)
    {
        return new ModbusRTU(config, loggerInfo);
    }
}
