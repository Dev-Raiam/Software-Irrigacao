using Toolbox.Industrial.Core.Communication.Modbus;
using Toolbox.Industrial.Driver.TekonBkp.Interfaces;
using Toolbox.Industrial.Driver.TekonBkp.Models;
using static Toolbox.Industrial.Core.Communication.Modbus.IModbusRTU;

namespace Toolbox.Industrial.Driver.TekonBkp
{
    public class TekonDriverFactory : ITekonDriverFactory
    {
        private readonly ITekonDispositivoFactory _dispositivoFactory;

        //private readonly IModbusFactory _modbusFactory;

        public TekonDriverFactory(ITekonDispositivoFactory dispositivoFactory)
        {
            _dispositivoFactory = dispositivoFactory;
        }

        public ITekonDriver CriarDriver(TekonDriverConfig config)
        {
            var modbus = Modbus.RtuMaster(
                new Configuration(
                    Port: config.Porta,
                    BaudRate: config.BaudRate,
                    DataBits: config.DataBits,
                    StopBits: config.StopBits,
                    Parity: config.Parity,
                    ReadTimeout: config.ReadTimeout,
                    WriteTimeout: config.WriteTimeout
                ),
                loggerInfo: "TekonDriver"
            );

            return new TekonDriver(modbus, _dispositivoFactory);
        }
    }
}
