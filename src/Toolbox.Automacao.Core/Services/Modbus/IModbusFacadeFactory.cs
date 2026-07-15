namespace Toolbox.Automacao.Core.Services.Modbus
{
    public interface IModbusFacadeFactory
    {
        IModbusFacade CriarRtuMaster(ModbusConfig config);
    }
}
