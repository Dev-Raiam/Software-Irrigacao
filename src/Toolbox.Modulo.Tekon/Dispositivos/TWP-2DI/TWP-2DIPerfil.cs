using Toolbox.Modulo.Tekon.Interfaces;
using Toolbox.Modulo.Tekon.Models;

namespace Toolbox.Modulo.Tekon.Dispositivos
{
    internal class TWP_2DIPerfil : ITekonDispositivoPerfil
    {
        public string Modelo => TekonConstants.Modelos.TWP_2DI;
        private const string ExceptionMessage =
            "O modelo escolhido não pode ser lido por esse método.";

        public ConfiguracaoLeitura ObterConfiguracaoLeituraDigital(string port) =>
            throw new NotSupportedException(ExceptionMessage);

        public ConfiguracaoLeituraDispositivo ObterConfiguracaoLeituraDispositivo() =>
            throw new NotSupportedException(ExceptionMessage);

        public ConfiguracaoLeitura ObterConfiguracaoLeituraAnalogica(string port) =>
            throw new NotSupportedException(ExceptionMessage);

        public ConfiguracaoLeitura ObterConfiguracaoLeituraAnalogica(string port, ushort index) =>
            throw new NotSupportedException(ExceptionMessage);

        public ConfiguracaoLeitura ObterConfiguracaoLeituraDigital(string port, ushort index) =>
            throw new NotSupportedException(ExceptionMessage);

        public ConfiguracaoEscritaDigital ObterConfiguracaoEscritaDigital(
            string port,
            ushort index
        ) => throw new NotSupportedException(ExceptionMessage);

        public ConfiguracaoLeituraDispositivo ObterConfiguracaoLeituraDispositivo(ushort index)
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
            return new TWP_2DI
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

                ContadorImpulsos_1 = (float)
                    Math.Round(
                        Conversor.ToFloat(
                            context.HoldingRegisters[10],
                            context.HoldingRegisters[9]
                        ),
                        2
                    ),
                ContadorImpulsos_2 = (float)
                    Math.Round(
                        Conversor.ToFloat(
                            context.HoldingRegisters[12],
                            context.HoldingRegisters[11]
                        ),
                        2
                    ),

                VersaoFirmware = context.HoldingRegisters[17],
                RevisaoVersao = context.HoldingRegisters[18],
                VersaoHardware = context.HoldingRegisters[19],
            };
        }

        private string IdentificarModelo(int modelo)
        {
            return modelo switch
            {
                50 => "TWP-2DI 868 MHZ",
                56 => "TWP-2DI 915 MHZ",
                _ => "Desconhecido",
            };
        }
    }
}
