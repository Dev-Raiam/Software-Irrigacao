using Toolbox.Automacao.Irrigacao.Marcas.Tekon;
using Toolbox.Automacao.Irrigacao.Modbus;
using Toolbox.Automacao.Irrigacao.Models;

namespace Toolbox.Automacao.Irrigacao.Marcas.Tekon.Modelos
{
    public class WGW420
    {
        public Analogica Analogica_1 { get; private set; }
        public Analogica Analogica_2 { get; private set; }
        public Analogica Analogica_3 { get; private set; }
        public Analogica Analogica_4 { get; private set; }
        public Analogica Analogica_5 { get; private set; }
        public Analogica Analogica_6 { get; private set; }
        public Analogica Analogica_7 { get; private set; }
        public Analogica Analogica_8 { get; private set; }

        public WGW420(ushort[] buffer)
        {
            Analogica_1 = new Analogica(
                valorMinimo: Conversor.ToFloat(buffer[1], buffer[0]),
                valorMaximo: Conversor.ToFloat(buffer[3], buffer[2]),
                desvioSaida: buffer[4],
                numeroTentativas: buffer[5],
                linkEnderecoModbus: buffer[6],
                valorCorrenteAtual: buffer[7] / 100.0f
            );

            Analogica_2 = new Analogica(
                valorMinimo: Conversor.ToFloat(buffer[9], buffer[8]),
                valorMaximo: Conversor.ToFloat(buffer[11], buffer[10]),
                desvioSaida: buffer[12],
                numeroTentativas: buffer[13],
                linkEnderecoModbus: buffer[14],
                valorCorrenteAtual: buffer[15] / 100.0f
            );

            Analogica_3 = new Analogica(
                valorMinimo: Conversor.ToFloat(buffer[17], buffer[16]),
                valorMaximo: Conversor.ToFloat(buffer[19], buffer[18]),
                desvioSaida: buffer[20],
                numeroTentativas: buffer[21],
                linkEnderecoModbus: buffer[22],
                valorCorrenteAtual: buffer[23] / 100.0f
            );

            Analogica_4 = new Analogica(
                valorMinimo: Conversor.ToFloat(buffer[25], buffer[24]),
                valorMaximo: Conversor.ToFloat(buffer[27], buffer[26]),
                desvioSaida: buffer[28],
                numeroTentativas: buffer[29],
                linkEnderecoModbus: buffer[30],
                valorCorrenteAtual: buffer[31] / 100.0f
            );

            Analogica_5 = new Analogica(
                valorMinimo: Conversor.ToFloat(buffer[33], buffer[32]),
                valorMaximo: Conversor.ToFloat(buffer[35], buffer[34]),
                desvioSaida: buffer[36],
                numeroTentativas: buffer[37],
                linkEnderecoModbus: buffer[38],
                valorCorrenteAtual: buffer[39] / 100.0f
            );

            Analogica_6 = new Analogica(
                valorMinimo: Conversor.ToFloat(buffer[41], buffer[40]),
                valorMaximo: Conversor.ToFloat(buffer[43], buffer[42]),
                desvioSaida: buffer[44],
                numeroTentativas: buffer[45],
                linkEnderecoModbus: buffer[46],
                valorCorrenteAtual: buffer[47] / 100.0f
            );

            Analogica_7 = new Analogica(
                valorMinimo: Conversor.ToFloat(buffer[49], buffer[48]),
                valorMaximo: Conversor.ToFloat(buffer[51], buffer[50]),
                desvioSaida: buffer[52],
                numeroTentativas: buffer[53],
                linkEnderecoModbus: buffer[54],
                valorCorrenteAtual: buffer[55] / 100.0f
            );

            Analogica_8 = new Analogica(
                valorMinimo: Conversor.ToFloat(buffer[57], buffer[56]),
                valorMaximo: Conversor.ToFloat(buffer[59], buffer[58]),
                desvioSaida: buffer[60],
                numeroTentativas: buffer[61],
                linkEnderecoModbus: buffer[62],
                valorCorrenteAtual: buffer[63] / 100.0f
            );
        }

        public class Analogica
        {
            public float ValorMinimo { get; private set; }
            public float ValorMaximo { get; private set; }
            public int DesvioSaida { get; private set; }
            public int NumeroTentativas { get; private set; }
            public int LinkEnderecoModbus { get; private set; }
            public float ValorCorrenteAtual { get; private set; }

            public Analogica(
                float valorMinimo,
                float valorMaximo,
                int desvioSaida,
                int numeroTentativas,
                int linkEnderecoModbus,
                float valorCorrenteAtual
            )
            {
                ValorMinimo = valorMinimo;
                ValorMaximo = valorMaximo;
                DesvioSaida = desvioSaida;
                NumeroTentativas = numeroTentativas;
                LinkEnderecoModbus = linkEnderecoModbus;
                ValorCorrenteAtual = valorCorrenteAtual;
            }
        }

        // public static IEnumerable<ConfiguracaoLeitura> ObterConfiguracaoLeitura()
        // {
        //     yield return new ConfiguracaoLeitura
        //     {
        //         StartAddress = ((1 - 1) * 8) + 1100 + 0,
        //         NumberOfRegister = 64,
        //     };
        // }

        public static ConfiguracaoLeitura ConfiguracaoHoldingRegisters()
        {
            return new ConfiguracaoLeitura
            {
                StartAddress = ((1 - 1) * 8) + 1100 + 0,
                NumberOfRegister = 64,
            };
        }

        public Telemetria ObterTelemetria(Guid moduloId, string modelo)
        {
            return new Telemetria
            {
                Id = moduloId,
                Timestamp = DateTime.Now,
                Metricas =
                [
                    new Metrica
                    {
                        Tipo = "analogica_1",
                        Valor = Analogica_1.ValorCorrenteAtual,
                        Unidade = "ma",
                    },
                    new Metrica
                    {
                        Tipo = "analogica_2",
                        Valor = Analogica_2.ValorCorrenteAtual,
                        Unidade = "ma",
                    },
                    new Metrica
                    {
                        Tipo = "analogica_3",
                        Valor = Analogica_3.ValorCorrenteAtual,
                        Unidade = "ma",
                    },
                    new Metrica
                    {
                        Tipo = "analogica_4",
                        Valor = Analogica_4.ValorCorrenteAtual,
                        Unidade = "ma",
                    },
                    new Metrica
                    {
                        Tipo = "analogica_5",
                        Valor = Analogica_5.ValorCorrenteAtual,
                        Unidade = "ma",
                    },
                    new Metrica
                    {
                        Tipo = "analogica_6",
                        Valor = Analogica_6.ValorCorrenteAtual,
                        Unidade = "ma",
                    },
                    new Metrica
                    {
                        Tipo = "analogica_7",
                        Valor = Analogica_7.ValorCorrenteAtual,
                        Unidade = "ma",
                    },
                    new Metrica
                    {
                        Tipo = "analogica_8",
                        Valor = Analogica_8.ValorCorrenteAtual,
                        Unidade = "ma",
                    },
                ],
                Metadados = new Metadados { Modelo = modelo },
            };
        }
    }
}
