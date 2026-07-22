using static Toolbox.Automacao.Core.Services.Modbus.IModbusRTU;

namespace Toolbox.Automacao.Core.Services.Modbus
{
    public static class Modbus
    {
        public static IModbusRTU RtuMaster(Configuration config, string loggerInfo)
        {
            return new ModbusRTU(config, loggerInfo);    
        }

        //public static IModbusTcp TcpMaster(Configuration config, string loggerInfo)
        //{
        //    return new ModbusTcp(config, loggerInfo);    
        //}
    }
}
