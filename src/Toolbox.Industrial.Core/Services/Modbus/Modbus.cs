using Toolbox.Automacao.Core.Services.ModbusRTU;

namespace Toolbox.Automacao.Core.Services.Modbus;

public static class Modbus
{
    public static IModbusRTU RtuMaster(Configuration config, string loggerInfo)
    {
        return new ModbusRTU.ModbusRTU(config, loggerInfo);
    }
}
