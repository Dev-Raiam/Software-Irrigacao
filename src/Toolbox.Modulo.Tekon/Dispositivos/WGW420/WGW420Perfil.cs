using Toolbox.Modulo.Tekon.Interfaces;
using Toolbox.Modulo.Tekon.Models;
using static Toolbox.Modulo.Tekon.Dispositivos.WGW420;

namespace Toolbox.Modulo.Tekon.Dispositivos
{
    internal class WGW420Perfil : ITekonDispositivoPerfil
    {
        public string Modelo => Modelos.WGW420;
        private const string ExceptionMensager =
            $"O modelo Escolhido não pode ser lido por esse metodo.";

        public ConfiguracaoLeituraDispositivo ObterConfiguracaoLeituraDispositivo(byte index)
            => throw new NotImplementedException(ExceptionMensager);

        ConfiguracaoLeituraDispositivo ITekonDispositivoPerfil.ObterConfiguracaoLeituraDispositivo()
            => throw new NotImplementedException(ExceptionMensager);
        public ConfiguracaoEscritaDigital ObterConfiguracaoEscritaDigital(string port)
            => throw new NotImplementedException(ExceptionMensager);

        public ConfiguracaoEscritaDigital ObterConfiguracaoEscritaDigital(
            string port,
            byte index
        ) => throw new NotImplementedException(ExceptionMensager);

        public ConfiguracaoLeitura ObterConfiguracaoLeituraAnalogica(string port, byte index) =>
            throw new NotImplementedException(ExceptionMensager);

        public ConfiguracaoLeitura ObterConfiguracaoLeituraAnalogica(string port) =>
            throw new NotImplementedException(ExceptionMensager);

        public ConfiguracaoLeitura ObterConfiguracaoLeituraDigital(string port, byte index) =>
            throw new NotImplementedException(ExceptionMensager);

        public ConfiguracaoLeitura ObterConfiguracaoLeituraDigital(string port) =>
            throw new NotImplementedException(ExceptionMensager);

        public ConfiguracaoLeitura ObterConfiguracaoLeituraTemperatura(string port) =>
            throw new NotImplementedException(ExceptionMensager);

        public ConfiguracaoLeitura ObterConfiguracaoLeituraTemperatura(string port, byte index) =>
            throw new NotImplementedException(ExceptionMensager);

        public ConfiguracaoLeitura ObterConfiguracaoLeituraDispositivo(int index) =>
            throw new NotImplementedException(ExceptionMensager);

        public ConfiguracaoLeitura ObterConfiguracaoLeituraDispositivo()
        {
            return new ConfiguracaoLeitura
            {
                StartAddress = ((1 - 1) * 8) + 1100 + 0,
                NumberOfPoints = 64,
            };
        }

        public ITekonDispositivoDado Parse(DispositivoContextoLeitura context)
        {
            return new WGW420
            {
                Analogica_1 = new Analogica(
                    valorMinimo: Conversor.ToFloat(
                        context.HoldingRegisters[1],
                        context.HoldingRegisters[0]
                    ),
                    valorMaximo: Conversor.ToFloat(
                        context.HoldingRegisters[3],
                        context.HoldingRegisters[2]
                    ),
                    desvioSaida: context.HoldingRegisters[4],
                    numeroTentativas: context.HoldingRegisters[5],
                    linkEnderecoModbus: context.HoldingRegisters[6],
                    valorCorrenteAtual: context.HoldingRegisters[7] / 100.0f
                ),

                Analogica_2 = new Analogica(
                    valorMinimo: Conversor.ToFloat(
                        context.HoldingRegisters[9],
                        context.HoldingRegisters[8]
                    ),
                    valorMaximo: Conversor.ToFloat(
                        context.HoldingRegisters[11],
                        context.HoldingRegisters[10]
                    ),
                    desvioSaida: context.HoldingRegisters[12],
                    numeroTentativas: context.HoldingRegisters[13],
                    linkEnderecoModbus: context.HoldingRegisters[14],
                    valorCorrenteAtual: context.HoldingRegisters[15] / 100.0f
                ),

                Analogica_3 = new Analogica(
                    valorMinimo: Conversor.ToFloat(
                        context.HoldingRegisters[17],
                        context.HoldingRegisters[16]
                    ),
                    valorMaximo: Conversor.ToFloat(
                        context.HoldingRegisters[19],
                        context.HoldingRegisters[18]
                    ),
                    desvioSaida: context.HoldingRegisters[20],
                    numeroTentativas: context.HoldingRegisters[21],
                    linkEnderecoModbus: context.HoldingRegisters[22],
                    valorCorrenteAtual: context.HoldingRegisters[23] / 100.0f
                ),

                Analogica_4 = new Analogica(
                    valorMinimo: Conversor.ToFloat(
                        context.HoldingRegisters[25],
                        context.HoldingRegisters[24]
                    ),
                    valorMaximo: Conversor.ToFloat(
                        context.HoldingRegisters[27],
                        context.HoldingRegisters[26]
                    ),
                    desvioSaida: context.HoldingRegisters[28],
                    numeroTentativas: context.HoldingRegisters[29],
                    linkEnderecoModbus: context.HoldingRegisters[30],
                    valorCorrenteAtual: context.HoldingRegisters[31] / 100.0f
                ),

                Analogica_5 = new Analogica(
                    valorMinimo: Conversor.ToFloat(
                        context.HoldingRegisters[33],
                        context.HoldingRegisters[32]
                    ),
                    valorMaximo: Conversor.ToFloat(
                        context.HoldingRegisters[35],
                        context.HoldingRegisters[34]
                    ),
                    desvioSaida: context.HoldingRegisters[36],
                    numeroTentativas: context.HoldingRegisters[37],
                    linkEnderecoModbus: context.HoldingRegisters[38],
                    valorCorrenteAtual: context.HoldingRegisters[39] / 100.0f
                ),

                Analogica_6 = new Analogica(
                    valorMinimo: Conversor.ToFloat(
                        context.HoldingRegisters[41],
                        context.HoldingRegisters[40]
                    ),
                    valorMaximo: Conversor.ToFloat(
                        context.HoldingRegisters[43],
                        context.HoldingRegisters[42]
                    ),
                    desvioSaida: context.HoldingRegisters[44],
                    numeroTentativas: context.HoldingRegisters[45],
                    linkEnderecoModbus: context.HoldingRegisters[46],
                    valorCorrenteAtual: context.HoldingRegisters[47] / 100.0f
                ),

                Analogica_7 = new Analogica(
                    valorMinimo: Conversor.ToFloat(
                        context.HoldingRegisters[49],
                        context.HoldingRegisters[48]
                    ),
                    valorMaximo: Conversor.ToFloat(
                        context.HoldingRegisters[51],
                        context.HoldingRegisters[50]
                    ),
                    desvioSaida: context.HoldingRegisters[52],
                    numeroTentativas: context.HoldingRegisters[53],
                    linkEnderecoModbus: context.HoldingRegisters[54],
                    valorCorrenteAtual: context.HoldingRegisters[55] / 100.0f
                ),

                Analogica_8 = new Analogica(
                    valorMinimo: Conversor.ToFloat(
                        context.HoldingRegisters[57],
                        context.HoldingRegisters[56]
                    ),
                    valorMaximo: Conversor.ToFloat(
                        context.HoldingRegisters[59],
                        context.HoldingRegisters[58]
                    ),
                    desvioSaida: context.HoldingRegisters[60],
                    numeroTentativas: context.HoldingRegisters[61],
                    linkEnderecoModbus: context.HoldingRegisters[62],
                    valorCorrenteAtual: context.HoldingRegisters[63] / 100.0f
                ),
            };
        }
    }
}
