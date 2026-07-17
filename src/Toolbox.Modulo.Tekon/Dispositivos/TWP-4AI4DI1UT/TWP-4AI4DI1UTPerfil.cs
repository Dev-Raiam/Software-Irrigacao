using Toolbox.Modulo.Tekon.Interfaces;
using Toolbox.Modulo.Tekon.Models;

namespace Toolbox.Modulo.Tekon.Dispositivos
{
    internal class TWP_4AI4DI1UTPerfil : ITekonDispositivoPerfil
    {
        public string Modelo => Modelos.TWP_4AI4DI1UT;
        private const string ExceptionMensager =
            $"O modelo Escolhido não pode ser lido por esse metodo.";

        public ConfiguracaoLeitura ObterConfiguracaoLeituraDigital(string port) =>
            throw new NotSupportedException(ExceptionMensager);

        public ConfiguracaoLeituraDispositivo ObterConfiguracaoLeituraDispositivo() =>
            throw new NotSupportedException(ExceptionMensager);

        public ConfiguracaoLeitura ObterConfiguracaoLeituraAnalogica(string port) =>
            throw new NotSupportedException(ExceptionMensager);

        public ConfiguracaoLeitura ObterConfiguracaoLeituraTemperatura(string port) =>
            throw new NotImplementedException(ExceptionMensager);

        public ConfiguracaoEscritaDigital ObterConfiguracaoEscritaDigital(string port) =>
            throw new NotImplementedException(ExceptionMensager);

        public ConfiguracaoLeitura ObterConfiguracaoLeituraAnalogica(string port, byte index)
        {
            return port switch
            {
                "A1" => new ConfiguracaoLeitura
                {
                    StartAddress = (ushort)(((index - 1) * 20) + 9),
                    NumberOfPoints = 3,
                },
                "A2" => new ConfiguracaoLeitura
                {
                    StartAddress = (ushort)(((index - 1) * 20) + 11),
                    NumberOfPoints = 3,
                },
                "A3" => new ConfiguracaoLeitura
                {
                    StartAddress = (ushort)(((index - 1) * 20) + 13),
                    NumberOfPoints = 3,
                },
                "A4" => new ConfiguracaoLeitura
                {
                    StartAddress = (ushort)(((index - 1) * 20) + 15),
                    NumberOfPoints = 3,
                },
                _ => throw new NotSupportedException("Porta não existe para o modelo escolhido"),
            };
        }

        /// Leitura de Entrada ou saida analogica em um metodo só
        public ConfiguracaoLeitura ObterConfiguracaoLeituraDigital(string port, byte index)
        {
            return port switch
            {
                "B1" => new ConfiguracaoLeitura
                {
                    StartAddress = (ushort)(((index - 1) * 16) + 3),
                    NumberOfPoints = 1,
                },
                "B2" => new ConfiguracaoLeitura
                {
                    StartAddress = (ushort)(((index - 1) * 16) + 4),
                    NumberOfPoints = 1,
                },
                "B3" => new ConfiguracaoLeitura
                {
                    StartAddress = (ushort)(((index - 1) * 16) + 5),
                    NumberOfPoints = 1,
                },
                "B4" => new ConfiguracaoLeitura
                {
                    StartAddress = (ushort)(((index - 1) * 16) + 6),
                    NumberOfPoints = 1,
                },
                "Q1" => new ConfiguracaoLeitura
                {
                    StartAddress = (ushort)(((index - 1) * 16) + 0),
                    NumberOfPoints = 1,
                },
                "Q2" => new ConfiguracaoLeitura
                {
                    StartAddress = (ushort)(((index - 1) * 16) + 1),
                    NumberOfPoints = 1,
                },
                "Q3" => new ConfiguracaoLeitura
                {
                    StartAddress = (ushort)(((index - 1) * 16) + 2),
                    NumberOfPoints = 1,
                },
                _ => throw new NotSupportedException("Porta não existe para o modelo escolhido"),
            };
        }

        public ConfiguracaoLeituraDispositivo ObterConfiguracaoLeituraDispositivo(byte index)
        {
            return new ConfiguracaoLeituraDispositivo
            {
                HoldingRegisters = new ConfiguracaoLeituraDispositivo.ConfiguracaoHoldingRegisters
                {
                    StartAddress = (ushort)(((index - 1) * 20) + 0),
                    NumberOfPoints = 20,
                },
                CoilRegisters = new ConfiguracaoLeituraDispositivo.ConfiguracaoCoilRegisters
                {
                    StartAddress = (ushort)(((index - 1) * 16) + 0),
                    NumberOfPoints = 7,
                },
            };
        }

        public ConfiguracaoEscritaDigital ObterConfiguracaoEscritaDigital(string port, byte index)
        {
            return port switch
            {
                "Q1" => new ConfiguracaoEscritaDigital
                {
                    CoilAddress = (ushort)(((index - 1) * 16) + 0),
                },
                _ => throw new NotSupportedException("Porta não existe para o modelo escolhido"),
            };
        }

        public ConfiguracaoLeitura ObterConfiguracaoLeituraTemperatura(string port, byte index)
        {
            return port switch
            {
                "UT" => new ConfiguracaoLeitura
                {
                    StartAddress = (ushort)(((index - 1) * 20) + 7),
                    NumberOfPoints = 3,
                },
                _ => throw new NotSupportedException("Porta não existe para o modelo escolhido"),
            };
        }

        public ITekonDispositivoDado Parse(DispositivoContextoLeitura context)
        {
            return new TWP_4AI4DI1UT
            {
                NumeroSerie = (long)(
                    context.HoldingRegisters[0] << 16 | context.HoldingRegisters[1]
                ),

                Modelo = IdentificarModelo(context.HoldingRegisters[2]),

                RSSI = context.HoldingRegisters[3] / -2,

                PeriodoComunicacao = context.HoldingRegisters[4],
                TempoDecorrido = context.HoldingRegisters[5],

                TensaoAlimentacao = context.HoldingRegisters[6] / 10.0f,

                TemperaturaExterna = (float)
                    Math.Round(
                        Conversor.ToFloat(context.HoldingRegisters[8], context.HoldingRegisters[7]),
                        2
                    ),
                ValorEntradaAnalogica_1 = (float)
                    Math.Round(
                        Conversor.ToFloat(
                            context.HoldingRegisters[10],
                            context.HoldingRegisters[9]
                        ),
                        2
                    ),
                ValorEntradaAnalogica_2 = (float)
                    Math.Round(
                        Conversor.ToFloat(
                            context.HoldingRegisters[12],
                            context.HoldingRegisters[11]
                        ),
                        2
                    ),
                ValorEntradaAnalogica_3 = (float)
                    Math.Round(
                        Conversor.ToFloat(
                            context.HoldingRegisters[14],
                            context.HoldingRegisters[13]
                        ),
                        2
                    ),
                ValorEntradaAnalogica_4 = (float)
                    Math.Round(
                        Conversor.ToFloat(
                            context.HoldingRegisters[16],
                            context.HoldingRegisters[15]
                        ),
                        2
                    ),

                VersaoFirmware = context.HoldingRegisters[17],
                RevisaoVersao = context.HoldingRegisters[18],
                VersaoHardware = context.HoldingRegisters[19],

                EstadoSaidaRemotaDigital = context.CoilRegisters[0],
                EstadoSaidaEnergiaExterna = context.CoilRegisters[1],
                EstadoEntradaInterruptor = context.CoilRegisters[2],
                EstadoEntradaDigital_1 = context.CoilRegisters[3],
                EstadoEntradaDigital_2 = context.CoilRegisters[4],
                EstadoEntradaDigital_3 = context.CoilRegisters[5],
                EstadoEntradaDigital_4 = context.CoilRegisters[6],
            };
        }

        public double ConverterValorAnalogico(ushort[] buffer, ConfiguracaoLeitura configuracao)
        {
            return Math.Round(Conversor.ToFloat(buffer[1], buffer[0]), 2);
        }

        public double ConverterValorTemperatura(ushort[] buffer, ConfiguracaoLeitura configuracao)
        {
            return Math.Round(Conversor.ToFloat(buffer[1], buffer[0]), 2);
        }

        private string IdentificarModelo(int modelo)
        {
            return modelo switch
            {
                37 => "Transmissor TWP_4AI4DI1UT 868 MHZ",
                38 => "Transmissor TWP_4AI4DI1UT 915 MHZ",
                _ => "Desconhecido",
            };
        }
    }
}
