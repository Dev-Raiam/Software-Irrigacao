using Toolbox.Automacao.Core.Services.Modbus;
using Toolbox.Modulo.Tekon.Interfaces;
using Toolbox.Modulo.Tekon.Models;
using static Toolbox.Automacao.Core.Services.Modbus.IModbusRTU;

namespace Toolbox.Modulo.Tekon
{
    public class TekonDriverFactory : ITekonDriverFactory
    {
        private readonly ITekonDispositivoFactory _dispositivoFactory;
        //private readonly IModbusFactory _modbusFactory;

        public TekonDriverFactory(
            ITekonDispositivoFactory dispositivoFactory)
        {
            _dispositivoFactory = dispositivoFactory;
        }

        public ITekonDriver CriarDriver(TekonDriverConfig config)
        {
            var modbus = Modbus.RtuMaster(new Configuration( 
                Port: config.Porta,
                BaudRate: config.BaudRate,
                DataBits: config.DataBits,
                StopBits: config.StopBits, 
                Parity: config.Parity, 
                ReadTimeout: config.ReadTimeout, 
                WriteTimeout: config.WriteTimeout
            ), loggerInfo: "TekonDriver");

            return new TekonDriver(modbus, _dispositivoFactory);
        }
    }
}
