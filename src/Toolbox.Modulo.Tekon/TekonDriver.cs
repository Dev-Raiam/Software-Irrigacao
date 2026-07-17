using System.Text.RegularExpressions;
using Toolbox.Automacao.Core.Services.Modbus;
using Toolbox.Modulo.Tekon.Interfaces;
using Toolbox.Modulo.Tekon.Models;

namespace Toolbox.Modulo.Tekon
{
    internal class TekonDriver : ITekonDriver, IDisposable
    {
        private readonly IModbusFacade _modbus;
        private readonly ITekonDispositivoFactory _factory;
        private bool _disposed;

        public TekonDriver(IModbusFacade modbus, ITekonDispositivoFactory factory)
        {
            _modbus = modbus;
            _factory = factory;
        }

        private void EnsureConnection()
        {
            _modbus.Conectar();
        }

        public async Task<ITekonDispositivoDado> LerDispositivo(string modelo, byte slaveAddress)
        {
            EnsureConnection();

            var dispositivo = _factory.CriarModelo(modelo);

            ushort[] bufferHolding = [];
            bool[] bufferCoils = [];

            var configuracao = dispositivo.ObterConfiguracaoLeituraDispositivo();
            var HoldingRegistersConfig = configuracao.HoldingRegisters;
            var CoilRegistersConfig = configuracao.CoilRegisters;

            if (HoldingRegistersConfig != null)
            {
                bufferHolding = await _modbus.LerHoldingRegistersAsync(
                    slaveAddress,
                    HoldingRegistersConfig.StartAddress,
                    HoldingRegistersConfig.NumberOfPoints
                );
            }

            if (CoilRegistersConfig != null)
            {
                bufferCoils = await _modbus.LerCoilsAsync(
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
            EnsureConnection();

            var dispositivo = _factory.CriarModelo(modelo);

            ushort[] bufferHolding = [];
            bool[] bufferCoils = [];

            var configuracao = dispositivo.ObterConfiguracaoLeituraDispositivo(index);
            var HoldingRegistersConfig = configuracao.HoldingRegisters;
            var CoilRegistersConfig = configuracao.CoilRegisters;

            if (HoldingRegistersConfig != null)
            {
                bufferHolding = await _modbus.LerHoldingRegistersAsync(
                    slaveAddress,
                    HoldingRegistersConfig.StartAddress,
                    HoldingRegistersConfig.NumberOfPoints
                );
            }

            if (CoilRegistersConfig != null)
            {
                bufferCoils = await _modbus.LerCoilsAsync(
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
            EnsureConnection();

            var dispositivo = _factory.CriarModelo(modelo);

            var configuracao = dispositivo.ObterConfiguracaoLeituraAnalogica(port, index);

            ushort[] bufferHolding = await _modbus.LerHoldingRegistersAsync(
                slaveAddress,
                configuracao.StartAddress,
                configuracao.NumberOfPoints
            );

            return dispositivo.ConverterValorAnalogico(bufferHolding, configuracao);
        }

        public async Task<double> LerPortaAnalogica(string modelo, byte slaveAddress, string port)
        {
            EnsureConnection();

            var dispositivo = _factory.CriarModelo(modelo);

            var configuracao = dispositivo.ObterConfiguracaoLeituraAnalogica(port);

            ushort[] bufferHolding = await _modbus.LerHoldingRegistersAsync(
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
            EnsureConnection();

            var dispositivo = _factory.CriarModelo(modelo);

            var configuracao = dispositivo.ObterConfiguracaoLeituraTemperatura(port, index);

            ushort[] bufferHolding = await _modbus.LerHoldingRegistersAsync(
                slaveAddress,
                configuracao.StartAddress,
                configuracao.NumberOfPoints
            );

            return dispositivo.ConverterValorTemperatura(bufferHolding, configuracao);
        }

        public async Task<double> LerPortaTemperatura(string modelo, byte slaveAddress, string port)
        {
            EnsureConnection();

            var dispositivo = _factory.CriarModelo(modelo);

            var configuracao = dispositivo.ObterConfiguracaoLeituraTemperatura(port);

            ushort[] bufferHolding = await _modbus.LerHoldingRegistersAsync(
                slaveAddress,
                configuracao.StartAddress,
                configuracao.NumberOfPoints
            );

            return dispositivo.ConverterValorTemperatura(bufferHolding, configuracao);
        }

        public async Task<bool> LerPortaDigital(string modelo, byte slaveAddress, string port)
        {
            EnsureConnection();

            var dispositivo = _factory.CriarModelo(modelo);

            var configuracao = dispositivo.ObterConfiguracaoLeituraDigital(port);

            bool[] bufferCoils = await _modbus.LerCoilsAsync(
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
            EnsureConnection();

            var dispositivo = _factory.CriarModelo(modelo);

            var configuracao = dispositivo.ObterConfiguracaoLeituraDigital(port, index);

            bool[] bufferCoils = await _modbus.LerCoilsAsync(
                slaveAddress,
                configuracao.StartAddress,
                configuracao.NumberOfPoints
            );

            return bufferCoils[0];
        }

        public Task EscreverPortaAnalogica(string modelo, byte slaveAddress, string port, int value)
        {
            throw new NotImplementedException();
        }

        public Task EscreverPortaAnalogica(
            string modelo,
            byte slaveAddress,
            byte index,
            string port,
            int value
        )
        {
            throw new NotImplementedException();
        }

        public async Task EscreverPortaDigital(
            string modelo,
            byte slaveAddress,
            string port,
            bool value
        )
        {
            EnsureConnection();

            var dispositivo = _factory.CriarModelo(modelo);

            var configuracao = dispositivo.ObterConfiguracaoEscritaDigital(port);

            await _modbus.EscreverCoilAsync(slaveAddress, configuracao.CoilAddress, value);
        }

        public async Task EscreverPortaDigital(
            string modelo,
            byte slaveAddress,
            byte index,
            string port,
            bool value
        )
        {
            EnsureConnection();

            var dispositivo = _factory.CriarModelo(modelo);

            var configuracao = dispositivo.ObterConfiguracaoEscritaDigital(port, index);

            await _modbus.EscreverCoilAsync(slaveAddress, configuracao.CoilAddress, value);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _modbus.Desconectar();

            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
