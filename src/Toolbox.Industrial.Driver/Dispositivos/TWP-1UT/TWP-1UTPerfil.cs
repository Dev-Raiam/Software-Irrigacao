using Toolbox.Modulo.Tekon.Exceptions;
using Toolbox.Modulo.Tekon.Interfaces;
using Toolbox.Modulo.Tekon.Models;

namespace Toolbox.Modulo.Tekon.Dispositivos
{
    internal class TWP_1UTPerfil : ITekonDispositivoPerfil
    {
        public string Modelo => Modelos.TWP_1UT;

        public ConfiguracaoLeitura ObterConfiguracaoLeituraDigital(string port) =>
            throw new TekonOperacaoNaoSuportadaException(
                $"{Modelo} não suporta leitura digital na porta {port}"
            );

        public ConfiguracaoLeituraDispositivo ObterConfiguracaoLeituraDispositivo() =>
            throw new TekonOperacaoNaoSuportadaException(
                "{Modelo} não suporta leitura de dispositivo sem índice"
            );

        public ConfiguracaoLeitura ObterConfiguracaoLeituraTemperatura(string port) =>
            throw new TekonOperacaoNaoSuportadaException(
                $"{Modelo} não suporta leitura de temperatura na porta {port}"
            );

        public ConfiguracaoLeitura ObterConfiguracaoLeituraAnalogica(string port) =>
            throw new TekonOperacaoNaoSuportadaException(
                $"{Modelo} não suporta leitura analógica na porta {port}"
            );

        public ConfiguracaoLeitura ObterConfiguracaoLeituraAnalogica(string port, byte index) =>
            throw new TekonOperacaoNaoSuportadaException(
                $"{Modelo} não suporta leitura analógica na porta {port} com índice {index}"
            );

        public ConfiguracaoLeitura ObterConfiguracaoLeituraDigital(string port, byte index) =>
            throw new TekonOperacaoNaoSuportadaException(
                $"{Modelo} não suporta leitura digital na porta {port} com índice {index}"
            );

        public ConfiguracaoLeitura ObterConfiguracaoLeituraTemperatura(string port, byte index) 
        {
            return port switch
            {
                "UT" => new ConfiguracaoLeitura
                {
                    StartAddress = (ushort)(((index - 1) * 20) + 9),
                    NumberOfPoints = 3
                },
                _ => throw new TekonPortaInvalidaException($"{Modelo} não suporta leitura analógica na porta {port}")
            };
        }

        public ConfiguracaoEscritaDigital ObterConfiguracaoEscritaDigital(string port) =>
            throw new TekonOperacaoNaoSuportadaException(
                $"{Modelo} não suporta escrita digital na porta {port}"
            );

        public ConfiguracaoEscritaDigital ObterConfiguracaoEscritaDigital(
            string port,
            byte index
        ) =>
            throw new TekonOperacaoNaoSuportadaException(
                $"{Modelo} não suporta escrita digital na porta {port} com índice {index}"
            );

        public ConfiguracaoLeituraDispositivo ObterConfiguracaoLeituraDispositivo(byte index)
        {
            return new ConfiguracaoLeituraDispositivo
            {
                HoldingRegisters = new ConfiguracaoLeituraDispositivo.ConfiguracaoHoldingRegisters
                {
                    StartAddress = (ushort)(((index - 1) * 20) + 0),
                    NumberOfPoints = 20,
                },
            };
        }

        public ITekonDispositivoDado Parse(DispositivoContextoLeitura context)
        {
            return new TWP_1UT
            {
                NumeroSerie = (long)(
                    context.HoldingRegisters[0] << 16 | context.HoldingRegisters[1]
                ),

                Modelo = IdentificarModelo(context.HoldingRegisters[2]),

                RSSI = context.HoldingRegisters[3] / -2,

                PeriodoComunicacao = context.HoldingRegisters[4],
                TempoDecorrido = context.HoldingRegisters[5],

                TensaoAlimentacao = context.HoldingRegisters[6] / 10.0f,

                TemperaturaInterna = (float)
                    Math.Round(
                        Conversor.ToFloat(context.HoldingRegisters[8], context.HoldingRegisters[7]),
                        2
                    ),
                TemperaturaExterna = (float)
                    Math.Round(
                        Conversor.ToFloat(
                            context.HoldingRegisters[10],
                            context.HoldingRegisters[9]
                        ),
                        2
                    ),

                VersaoFirmware = context.HoldingRegisters[17],
                RevisaoVersao = context.HoldingRegisters[18],
                VersaoHardware = context.HoldingRegisters[19],
            };
        }

        public double ConverterValorAnalogico(ushort[] buffer, ConfiguracaoLeitura configuracao)
        {
            throw new TekonOperacaoNaoSuportadaException($"{Modelo} não suporta leitura analógica");
        }

        public double ConverterValorTemperatura(ushort[] buffer, ConfiguracaoLeitura configuracao)
        {
            return Math.Round(Conversor.ToFloat(buffer[1], buffer[0]), 2);
        }

        private string IdentificarModelo(int modelo)
        {
            return modelo switch
            {
                45 => "TWP-1UT 868 MHZ",
                51 => "TWP-1UT 915 MHZ",
                _ => "Desconhecido",
            };
        }
    }
}
