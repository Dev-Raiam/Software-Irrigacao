using Toolbox.Industrial.Core.Communication.Modbus;
using Toolbox.Industrial.Driver.TekonBkp.Interfaces;
using Toolbox.Industrial.Driver.TekonBkp.Models;

namespace Toolbox.Industrial.Driver.TekonBkp
{
    internal class TekonDriver : ITekonDriver, IDisposable
    {
        private readonly IModbusRTU _modbus;
        private readonly ITekonDispositivoFactory _factory;
        private bool _disposed;

        public TekonDriver(IModbusRTU modbus, ITekonDispositivoFactory factory)
        {
            _modbus = modbus;
            _factory = factory;
        }

        public async Task<ITekonDispositivoDado> LerDispositivo(string modelo, byte slaveAddress)
        {
            var dispositivo = _factory.CriarModelo(modelo);

            ushort[] bufferHolding = [];
            bool[] bufferCoils = [];

            var configuracao = dispositivo.ObterConfiguracaoLeituraDispositivo();
            var HoldingRegistersConfig = configuracao.HoldingRegisters;
            var CoilRegistersConfig = configuracao.CoilRegisters;

            if (HoldingRegistersConfig != null)
            {
                    
                bufferHolding = await _modbus.ReadHoldingRegistersAsync(
                    slaveAddress,
                    HoldingRegistersConfig.StartAddress,
                    HoldingRegistersConfig.NumberOfPoints
                );
            }

            if (CoilRegistersConfig != null)
            {
                bufferCoils = await _modbus.ReadCoilsAsync(
                    slaveAddress,
                    CoilRegistersConfig.StartAddress,
                    CoilRegistersConfig.NumberOfPoints
                );
            }

            var contexto = new DispositivoContextoLeitura
            {
                HoldingRegisters = bufferHolding,
                CoilRegisters = bufferCoils,
            };

            return dispositivo.Parse(contexto);
        }

        public async Task<ITekonDispositivoDado> LerDispositivo(
            string modelo,
            byte slaveAddress,
            byte index
        )
        {
            var dispositivo = _factory.CriarModelo(modelo);

            ushort[] bufferHolding = [];
            bool[] bufferCoils = [];

            var configuracao = dispositivo.ObterConfiguracaoLeituraDispositivo(index);
            var HoldingRegistersConfig = configuracao.HoldingRegisters;
            var CoilRegistersConfig = configuracao.CoilRegisters;

            if (HoldingRegistersConfig != null)
            {
                bufferHolding = await _modbus.ReadHoldingRegistersAsync(
                    slaveAddress,
                    HoldingRegistersConfig.StartAddress,
                    HoldingRegistersConfig.NumberOfPoints
                );
            }

            if (CoilRegistersConfig != null)
            {
                bufferCoils = await _modbus.ReadCoilsAsync(
                    slaveAddress,
                    CoilRegistersConfig.StartAddress,
                    CoilRegistersConfig.NumberOfPoints
                );
            }

            var contexto = new DispositivoContextoLeitura
            {
                HoldingRegisters = bufferHolding,
                CoilRegisters = bufferCoils,
            };

            return dispositivo.Parse(contexto);
        }

        public async Task<double> LerPortaAnalogica(
            string modelo,
            byte slaveAddress,
            byte index,
            string port
        )
        {
            var dispositivo = _factory.CriarModelo(modelo);

            var configuracao = dispositivo.ObterConfiguracaoLeituraAnalogica(port, index);
            ushort[] bufferHolding = await _modbus.ReadHoldingRegistersAsync(
                slaveAddress,
                configuracao.StartAddress,
                configuracao.NumberOfPoints
            );

            return dispositivo.ConverterValorAnalogico(bufferHolding, configuracao);
        }

        public async Task<double> LerPortaAnalogica(string modelo, byte slaveAddress, string port)
        {
            var dispositivo = _factory.CriarModelo(modelo);

            var configuracao = dispositivo.ObterConfiguracaoLeituraAnalogica(port);
            ushort[] bufferHolding = await _modbus.ReadHoldingRegistersAsync(
                slaveAddress,
                configuracao.StartAddress,
                configuracao.NumberOfPoints
            );

            return dispositivo.ConverterValorAnalogico(bufferHolding, configuracao);
        }

        public async Task<double> LerPortaTemperatura(
            string modelo,
            byte slaveAddress,
            byte index,
            string port
        )
        {
            var dispositivo = _factory.CriarModelo(modelo);

            var configuracao = dispositivo.ObterConfiguracaoLeituraTemperatura(port, index);
            ushort[] bufferHolding = await _modbus.ReadHoldingRegistersAsync(
                slaveAddress,
                configuracao.StartAddress,
                configuracao.NumberOfPoints
            );

            return dispositivo.ConverterValorTemperatura(bufferHolding, configuracao);
        }

        public async Task<double> LerPortaTemperatura(string modelo, byte slaveAddress, string port)
        {
            var dispositivo = _factory.CriarModelo(modelo);

            var configuracao = dispositivo.ObterConfiguracaoLeituraTemperatura(port);
            ushort[] bufferHolding = await _modbus.ReadHoldingRegistersAsync(
                slaveAddress,
                configuracao.StartAddress,
                configuracao.NumberOfPoints
            );

            return dispositivo.ConverterValorTemperatura(bufferHolding, configuracao);
        }

        public async Task<bool> LerPortaDigital(string modelo, byte slaveAddress, string port)
        {
            var dispositivo = _factory.CriarModelo(modelo);

            var configuracao = dispositivo.ObterConfiguracaoLeituraDigital(port);
            bool[] bufferCoils = await _modbus.ReadCoilsAsync(
                slaveAddress,
                configuracao.StartAddress,
                configuracao.NumberOfPoints
            );

            return bufferCoils[0];
        }

        public async Task<bool> LerPortaDigital(
            string modelo,
            byte slaveAddress,
            byte index,
            string port
        )
        {
            var dispositivo = _factory.CriarModelo(modelo);

            var configuracao = dispositivo.ObterConfiguracaoLeituraDigital(port, index);
            bool[] bufferCoils = await _modbus.ReadCoilsAsync(
                slaveAddress,
                configuracao.StartAddress,
                configuracao.NumberOfPoints
            );

            return bufferCoils[0];
        }

        //public Task EscreverPortaAnalogica(string modelo, byte slaveAddress, string port, int value)
        //{
        //    throw new NotImplementedException();
        //}

        //public Task EscreverPortaAnalogica(
        //    string modelo,
        //    byte slaveAddress,
        //    byte index,
        //    string port,
        //    int value
        //)
        //{
        //    throw new NotImplementedException();
        //}

        public async Task EscreverPortaDigital(
            string modelo,
            byte slaveAddress,
            string port,
            bool value
        )
        {
            var dispositivo = _factory.CriarModelo(modelo);
            var configuracao = dispositivo.ObterConfiguracaoEscritaDigital(port);
            await _modbus.WriteCoilAsync(slaveAddress, configuracao.CoilAddress, value);
        }

        public async Task EscreverPortaDigital(
            string modelo,
            byte slaveAddress,
            byte index,
            string port,
            bool value
        )
        {
            var dispositivo = _factory.CriarModelo(modelo);

            var configuracao = dispositivo.ObterConfiguracaoEscritaDigital(port, index);
            await _modbus.WriteCoilAsync(slaveAddress, configuracao.CoilAddress, value);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _modbus.Dispose();
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
