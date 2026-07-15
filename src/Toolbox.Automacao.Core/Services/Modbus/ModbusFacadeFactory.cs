namespace Toolbox.Automacao.Core.Services.Modbus
{
    public class ModbusFacadeFactory : IModbusFacadeFactory
    {
        public IModbusFacade CriarRtuMaster(ModbusConfig config)
        {
            return new ModbusFacade(config);    
        }
    }
}
