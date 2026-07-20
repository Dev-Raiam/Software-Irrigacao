namespace Toolbox.Automacao.Core.Services.Modbus
{
    public interface IModbusFactory
    {
        IModbus CriarRtuMaster(ModbusConfig config);
    }
    public class ModbusFactory : IModbusFactory
    {
        public IModbus CriarRtuMaster(ModbusConfig config)
        {
            return new Modbus(config);    
        }
    }
}
