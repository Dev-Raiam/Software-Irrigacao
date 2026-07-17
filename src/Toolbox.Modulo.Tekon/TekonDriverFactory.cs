using Toolbox.Automacao.Core.Services.Modbus;
using Toolbox.Modulo.Tekon.Interfaces;
using Toolbox.Modulo.Tekon.Models;

namespace Toolbox.Modulo.Tekon
{
    public class TekonDriverFactory : ITekonDriverFactory
    {
        private readonly ITekonDispositivoFactory _dispositivoFactory;
        private readonly IModbusFacadeFactory _modbusFactory;

        public TekonDriverFactory(
            ITekonDispositivoFactory dispositivoFactory,
            IModbusFacadeFactory modbusFactory)
        {
            _dispositivoFactory = dispositivoFactory;
            _modbusFactory = modbusFactory;
        }

        public ITekonDriver CriarDriver(TekonDriverConfig config)
        {
            var modbus = _modbusFactory.CriarRtuMaster(new ModbusConfig 
            { 
                Porta = config.Porta,
                BaudRate = config.BaudRate,
                DataBits = config.DataBits,
                StopBits = config.StopBits, 
                Parity = config.Parity, 
                ReadTimeout = config.ReadTimeout, 
                WriteTimeout = config.WriteTimeout
            });

            return new TekonDriver(modbus, _dispositivoFactory);
        }
    }
}
