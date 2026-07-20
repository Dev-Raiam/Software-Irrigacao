using System.Text.RegularExpressions;
using Toolbox.Automacao.Core.Services.Modbus;
using Toolbox.Automacao.Core.Services.Modbus.Exceptions;
using Toolbox.Modulo.Tekon.Exceptions;
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
            try
            {
                _modbus.Conectar();
            }
            catch (ModbusConexaoException ex)
            {
                throw new TekonComunicacaoException(
                    "Não foi possível estabelecer conexão com o dispositivo Tekon. Verifique se o dispositivo está ligado e conectado.",
                    ex
                );
            }
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
                try
                {
                    bufferHolding = await _modbus.LerHoldingRegistersAsync(
                        slaveAddress,
                        HoldingRegistersConfig.StartAddress,
                        HoldingRegistersConfig.NumberOfPoints
                    );
                }
                catch (ModbusLeituraException ex)
                {
                    throw new TekonLeituraException(
                        $"Não foi possível ler dados do dispositivo modelo {modelo}. Verifique a conexão e tente novamente.",
                        ex
                    );
                }
            }

            if (CoilRegistersConfig != null)
            {
                try
                {
                    bufferCoils = await _modbus.LerCoilsAsync(
                        slaveAddress,
                        CoilRegistersConfig.StartAddress,
                        CoilRegistersConfig.NumberOfPoints
                    );
                }
                catch (ModbusLeituraException ex)
                {
                    throw new TekonLeituraException(
                        $"Não foi possível ler dados do dispositivo modelo {modelo}. Verifique a conexão e tente novamente.",
                        ex
                    );
                }
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
                try
                {
                    bufferHolding = await _modbus.LerHoldingRegistersAsync(
                        slaveAddress,
                        HoldingRegistersConfig.StartAddress,
                        HoldingRegistersConfig.NumberOfPoints
                    );
                }
                catch (ModbusLeituraException ex)
                {
                    throw new TekonLeituraException(
                        $"Não foi possível ler dados do dispositivo modelo {modelo}. Verifique a conexão e tente novamente.",
                        ex
                    );
                }
            }

            if (CoilRegistersConfig != null)
            {
                try
                {
                    bufferCoils = await _modbus.LerCoilsAsync(
                        slaveAddress,
                        CoilRegistersConfig.StartAddress,
                        CoilRegistersConfig.NumberOfPoints
                    );
                }
                catch (ModbusLeituraException ex)
                {
                    throw new TekonLeituraException(
                        $"Não foi possível ler dados do dispositivo modelo {modelo}. Verifique a conexão e tente novamente.",
                        ex
                    );
                }
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

            try
            {
                ushort[] bufferHolding = await _modbus.LerHoldingRegistersAsync(
                    slaveAddress,
                    configuracao.StartAddress,
                    configuracao.NumberOfPoints
                );

                return dispositivo.ConverterValorAnalogico(bufferHolding, configuracao);
            }
            catch (ModbusLeituraException ex)
            {
                throw new TekonLeituraException(
                    $"Não foi possível ler a porta analógica {port} do dispositivo modelo {modelo}. Verifique a conexão.",
                    ex
                );
            }
        }

        public async Task<double> LerPortaAnalogica(string modelo, byte slaveAddress, string port)
        {
            EnsureConnection();

            var dispositivo = _factory.CriarModelo(modelo);

            var configuracao = dispositivo.ObterConfiguracaoLeituraAnalogica(port);

            try
            {
                ushort[] bufferHolding = await _modbus.LerHoldingRegistersAsync(
                    slaveAddress,
                    configuracao.StartAddress,
                    configuracao.NumberOfPoints
                );

                return dispositivo.ConverterValorAnalogico(bufferHolding, configuracao);
            }
            catch (ModbusLeituraException ex)
            {
                throw new TekonLeituraException(
                    $"Não foi possível ler a porta analógica {port} do dispositivo modelo {modelo}. Verifique a conexão.",
                    ex
                );
            }
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

            try
            {
                ushort[] bufferHolding = await _modbus.LerHoldingRegistersAsync(
                    slaveAddress,
                    configuracao.StartAddress,
                    configuracao.NumberOfPoints
                );

                return dispositivo.ConverterValorTemperatura(bufferHolding, configuracao);
            }
            catch (ModbusLeituraException ex)
            {
                throw new TekonLeituraException(
                    $"Não foi possível ler a porta de temperatura {port} do dispositivo modelo {modelo}. Verifique a conexão.",
                    ex
                );
            }
        }

        public async Task<double> LerPortaTemperatura(string modelo, byte slaveAddress, string port)
        {
            EnsureConnection();

            var dispositivo = _factory.CriarModelo(modelo);

            var configuracao = dispositivo.ObterConfiguracaoLeituraTemperatura(port);

            try
            {
                ushort[] bufferHolding = await _modbus.LerHoldingRegistersAsync(
                    slaveAddress,
                    configuracao.StartAddress,
                    configuracao.NumberOfPoints
                );

                return dispositivo.ConverterValorTemperatura(bufferHolding, configuracao);
            }
            catch (ModbusLeituraException ex)
            {
                throw new TekonLeituraException(
                    $"Não foi possível ler a porta de temperatura {port} do dispositivo modelo {modelo}. Verifique a conexão.",
                    ex
                );
            }
        }

        public async Task<bool> LerPortaDigital(string modelo, byte slaveAddress, string port)
        {
            EnsureConnection();

            var dispositivo = _factory.CriarModelo(modelo);

            var configuracao = dispositivo.ObterConfiguracaoLeituraDigital(port);

            try
            {
                bool[] bufferCoils = await _modbus.LerCoilsAsync(
                    slaveAddress,
                    configuracao.StartAddress,
                    configuracao.NumberOfPoints
                );

                return bufferCoils[0];
            }
            catch (ModbusLeituraException ex)
            {
                throw new TekonLeituraException(
                    $"Não foi possível ler a porta digital {port} do dispositivo modelo {modelo}. Verifique a conexão.",
                    ex
                );
            }
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

            try
            {
                bool[] bufferCoils = await _modbus.LerCoilsAsync(
                    slaveAddress,
                    configuracao.StartAddress,
                    configuracao.NumberOfPoints
                );

                return bufferCoils[0];
            }
            catch (ModbusLeituraException ex)
            {
                throw new TekonLeituraException(
                    $"Não foi possível ler a porta digital {port} do dispositivo modelo {modelo}. Verifique a conexão.",
                    ex
                );
            }
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
            EnsureConnection();

            var dispositivo = _factory.CriarModelo(modelo);

            var configuracao = dispositivo.ObterConfiguracaoEscritaDigital(port);

            try
            {
                await _modbus.EscreverCoilAsync(slaveAddress, configuracao.CoilAddress, value);
            }
            catch (ModbusEscritaException ex)
            {
                throw new TekonEscritaException(
                    $"Não foi possível escrever na porta digital {port} do dispositivo modelo {modelo}. Verifique a conexão.",
                    ex
                );
            }
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

            try
            {
                await _modbus.EscreverCoilAsync(slaveAddress, configuracao.CoilAddress, value);
            }
            catch (ModbusEscritaException ex)
            {
                throw new TekonEscritaException(
                    $"Não foi possível escrever na porta digital {port} do dispositivo modelo {modelo}. Verifique a conexão.",
                    ex
                );
            }
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
