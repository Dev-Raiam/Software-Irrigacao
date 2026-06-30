using Toolbox.Automacao.Irrigacao.Modbus;
using Toolbox.Automacao.Irrigacao.Models;

namespace Toolbox.Automacao.Irrigacao.Marcas.Tekon.Modelos
{
    public class TWP_4AI4DI1UT
    {
        public long NumeroSerie { get; private set; }
        public string Modelo { get; set; } = null!;
        public int RSSI { get; private set; }
        public int PeriodoComunicacao { get; private set; }
        public int TempoDecorrido { get; private set; }
        public float TensaoAlimentacao { get; private set; }
        public float TemperaturaExterna { get; private set; }
        public float ValorEntradaAnalogica_1 { get; private set; }
        public float ValorEntradaAnalogica_2 { get; private set; }
        public float ValorEntradaAnalogica_3 { get; private set; }
        public float ValorEntradaAnalogica_4 { get; private set; }
        public int VersaoFirmware { get; private set; }
        public int RevisaoVersao { get; private set; }
        public int VersaoHardware { get; private set; }
        public bool EstadoSaidaRemotaDigital { get; private set; }
        public bool EstadoSaidaEnergiaExterna { get; private set; }
        public bool EstadoEntradaInterruptor { get; private set; }
        public bool EstadoEntradaDigital_1 { get; private set; }
        public bool EstadoEntradaDigital_2 { get; private set; }
        public bool EstadoEntradaDigital_3 { get; private set; }
        public bool EstadoEntradaDigital_4 { get; private set; }

        public TWP_4AI4DI1UT(ushort[] bufferHolding, bool[] bufferCoils)
        {
            NumeroSerie = (long)(bufferHolding[0] << 16 | bufferHolding[1]);

            Modelo = IdentificarModelo(bufferHolding[2]);

            RSSI = bufferHolding[3] / -2;

            PeriodoComunicacao = bufferHolding[4];
            TempoDecorrido = bufferHolding[5];

            TensaoAlimentacao = bufferHolding[6] / 10.0f;

            TemperaturaExterna = (float)
                Math.Round(Conversor.ToFloat(bufferHolding[8], bufferHolding[7]), 2);
            ValorEntradaAnalogica_1 = (float)
                Math.Round(Conversor.ToFloat(bufferHolding[10], bufferHolding[9]), 2);
            ValorEntradaAnalogica_2 = (float)
                Math.Round(Conversor.ToFloat(bufferHolding[12], bufferHolding[11]), 2);
            ValorEntradaAnalogica_3 = (float)
                Math.Round(Conversor.ToFloat(bufferHolding[14], bufferHolding[13]), 2);
            ValorEntradaAnalogica_4 = (float)
                Math.Round(Conversor.ToFloat(bufferHolding[16], bufferHolding[15]), 2);

            VersaoFirmware = bufferHolding[17];
            RevisaoVersao = bufferHolding[18];
            VersaoHardware = bufferHolding[19];

            if (bufferCoils.Any())
            {
                EstadoSaidaRemotaDigital = bufferCoils[0];
                EstadoSaidaEnergiaExterna = bufferCoils[1];
                EstadoEntradaInterruptor = bufferCoils[2];
                EstadoEntradaDigital_1 = bufferCoils[3];
                EstadoEntradaDigital_2 = bufferCoils[4];
                EstadoEntradaDigital_3 = bufferCoils[5];
                EstadoEntradaDigital_4 = bufferCoils[6];
            }
        }

        public static ConfiguracaoLeitura ConfiguracaoHoldingRegisters(byte index)
        {
            return new ConfiguracaoLeitura
            {
                StartAddress = (ushort)(((index - 1) * 20) + 0),
                NumberOfRegister = 20,
            };
        }

        public static ConfiguracaoLeitura ConfiguracaoCoilsRegisters(byte index)
        {
            return new ConfiguracaoLeitura
            {
                StartAddress = (ushort)(((index - 1) * 16) + 0),
                NumberOfRegister = 10,
            };
        }

        public Telemetria ObterTelemetria(Guid moduloId)
        {
            return new Telemetria
            {
                Id = moduloId,
                Timestamp = DateTime.Now,
                Status = (TempoDecorrido > 10) ? "offline" : "online",
                Metricas =
                [
                    new Metrica
                    {
                        Tipo = "temperatura",
                        Valor = TemperaturaExterna,
                        Unidade = "°C",
                    },
                    new Metrica
                    {
                        Tipo = "tensao",
                        Valor = TensaoAlimentacao,
                        Unidade = "V",
                    },
                    new Metrica
                    {
                        Tipo = "rssi",
                        Valor = RSSI,
                        Unidade = "dBm",
                    },
                    new Metrica
                    {
                        Tipo = "estado-saida-remota-digital",
                        Valor = EstadoSaidaRemotaDigital,
                        Unidade = "boolean",
                    },
                    new Metrica
                    {
                        Tipo = "estado-saida-energia-externa",
                        Valor = EstadoSaidaEnergiaExterna,
                        Unidade = "boolean",
                    },
                    new Metrica
                    {
                        Tipo = "estado-entrada-interruptor",
                        Valor = EstadoEntradaInterruptor,
                        Unidade = "boolean",
                    },
                    new Metrica
                    {
                        Tipo = "estado-entrada-digital_1",
                        Valor = EstadoEntradaDigital_1,
                        Unidade = "boolean",
                    },
                    new Metrica
                    {
                        Tipo = "estado-entrada-digital_2",
                        Valor = EstadoEntradaDigital_2,
                        Unidade = "boolean",
                    },
                    new Metrica
                    {
                        Tipo = "estado-entrada-digital_3",
                        Valor = EstadoEntradaDigital_3,
                        Unidade = "boolean",
                    },
                    new Metrica
                    {
                        Tipo = "estado-entrada-digital_4",
                        Valor = EstadoEntradaDigital_4,
                        Unidade = "boolean",
                    },
                ],
                Metadados = new Metadados { Modelo = Modelo, VersaoFirmware = VersaoFirmware },
            };
        }

        private static string IdentificarModelo(int modelo)
        {
            return modelo switch
            {
                37 => "Transmissor TWP_4AI4DI1UT 868 MHZ",
                38 => "Transmissor TWP_4AI4DI1UT 915 MHZ",
                _ => "",
            };
        }
    }
}
