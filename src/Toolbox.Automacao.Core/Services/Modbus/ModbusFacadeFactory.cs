namespace Toolbox.Automacao.Core.Services.Modbus
{
    public interface IModbusFacadeFactory
    {
        IModbusFacade CriarRtuMaster(ModbusConfig config);
    }
    public class ModbusFacadeFactory : IModbusFacadeFactory
    {
        public IModbusFacade CriarRtuMaster(ModbusConfig config)
        {
            return new ModbusFacade(config);    
        }
    }
}
