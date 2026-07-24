using Toolbox.Industrial.Core.Services.ModbusRTU;

namespace Toolbox.Industrial.Core.Services.Modbus;

public static class Modbus
{
    public static IModbusRTU RtuMaster(Configuration config, string loggerInfo)
    {
        return new ModbusRTU.ModbusRTU(config, loggerInfo);
    }
}
