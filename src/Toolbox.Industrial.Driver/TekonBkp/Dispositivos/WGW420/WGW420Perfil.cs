using Toolbox.Industrial.Driver.TekonBkp.Exceptions;
using Toolbox.Industrial.Driver.TekonBkp.Interfaces;
using Toolbox.Industrial.Driver.TekonBkp.Models;
using static Toolbox.Industrial.Driver.TekonBkp.Dispositivos.WGW420;

namespace Toolbox.Industrial.Driver.TekonBkp.Dispositivos
{
    internal class WGW420Perfil : ITekonDispositivoPerfil
    {
        public string Modelo => Modelos.WGW420;

        public ConfiguracaoLeituraDispositivo ObterConfiguracaoLeituraDispositivo(byte index) =>
            throw new TekonOperacaoNaoSuportadaException(
                $"{Modelo} não suporta leitura de dispositivo com índice {index}"
            );

        ConfiguracaoLeituraDispositivo ITekonDispositivoPerfil.ObterConfiguracaoLeituraDispositivo() =>
            throw new TekonOperacaoNaoSuportadaException(
                $"{Modelo} não suporta leitura de dispositivo"
            );

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

        public ConfiguracaoLeitura ObterConfiguracaoLeituraAnalogica(string port, byte index) =>
            throw new TekonOperacaoNaoSuportadaException(
                $"{Modelo} não suporta leitura analógica na porta {port} com índice {index}"
            );

        public ConfiguracaoLeitura ObterConfiguracaoLeituraAnalogica(string port)
        {
            return port switch
            {
                "A4" => new ConfiguracaoLeitura
                {
                    StartAddress = ((1 - 1) * 8) + 1100 + 7,
                    NumberOfPoints = 1,
                },
                "A5" => new ConfiguracaoLeitura
                {
                    StartAddress = ((2 - 1) * 8) + 1100 + 7,
                    NumberOfPoints = 1,
                },
                "A6" => new ConfiguracaoLeitura
                {
                    StartAddress = ((3 - 1) * 8) + 1100 + 7,
                    NumberOfPoints = 1,
                },
                "A10" => new ConfiguracaoLeitura
                {
                    StartAddress = ((4 - 1) * 8) + 1100 + 7,
                    NumberOfPoints = 1,
                },
                "A11" => new ConfiguracaoLeitura
                {
                    StartAddress = ((5 - 1) * 8) + 1100 + 7,
                    NumberOfPoints = 1,
                },
                "A12" => new ConfiguracaoLeitura
                {
                    StartAddress = ((6 - 1) * 8) + 1100 + 7,
                    NumberOfPoints = 1,
                },
                "A16" => new ConfiguracaoLeitura
                {
                    StartAddress = ((7 - 1) * 8) + 1100 + 7,
                    NumberOfPoints = 1,
                },
                "A17" => new ConfiguracaoLeitura
                {
                    StartAddress = ((8 - 1) * 8) + 1100 + 7,
                    NumberOfPoints = 1,
                },
                _ => throw new TekonPortaInvalidaException(
                    $"Porta {port} não existe para o modelo WGW420"
                ),
            };
        }

        public ConfiguracaoLeitura ObterConfiguracaoLeituraDigital(string port, byte index) =>
            throw new TekonOperacaoNaoSuportadaException(
                $"{Modelo} não suporta leitura digital na porta {port} com índice {index}"
            );

        public ConfiguracaoLeitura ObterConfiguracaoLeituraDigital(string port) =>
            throw new TekonOperacaoNaoSuportadaException(
                $"{Modelo} não suporta leitura digital na porta {port}"
            );

        public ConfiguracaoLeitura ObterConfiguracaoLeituraTemperatura(string port) =>
            throw new TekonOperacaoNaoSuportadaException(
                $"{Modelo} não suporta leitura de temperatura na porta {port}"
            );

        public ConfiguracaoLeitura ObterConfiguracaoLeituraTemperatura(string port, byte index) =>
            throw new TekonOperacaoNaoSuportadaException(
                $"{Modelo} não suporta leitura de temperatura na porta {port} com índice {index}"
            );

        public ConfiguracaoLeitura ObterConfiguracaoLeituraDispositivo(int index) =>
            throw new TekonOperacaoNaoSuportadaException(
                $"{Modelo} não suporta leitura de dispositivo com índice {index}"
            );

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

        public double ConverterValorAnalogico(ushort[] buffer, ConfiguracaoLeitura configuracao)
        {
            return Math.Round(buffer[0] / 100.0, 2);
        }

        public double ConverterValorTemperatura(ushort[] buffer, ConfiguracaoLeitura configuracao)
        {
            throw new TekonOperacaoNaoSuportadaException(
                $"{Modelo} não suporta leitura de temperatura"
            );
        }
    }
}
