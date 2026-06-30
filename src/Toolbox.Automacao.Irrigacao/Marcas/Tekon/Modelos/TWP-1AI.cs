using System;
using System.Collections.Generic;
using System.Text;
using Toolbox.Automacao.Irrigacao.Models;

namespace Toolbox.Automacao.Irrigacao.Marcas.Tekon.Modelos
{
    public class TWP_1AI
    {
        public long NumeroSerie { get; private set; }
        public string Modelo { get; set; } = null!;
        public int RSSI { get; private set; }
        public int PeriodoComunicacao { get; private set; }
        public int TempoDecorrido { get; private set; }
        public float TensaoAlimentacao { get; private set; }
        public float TemperaturaInterna { get; private set; }
        public float ValorEntradaAnalogica_1 { get; private set; }
        public int VersaoFirmware { get; private set; }
        public int RevisaoVersao { get; private set; }
        public int VersaoHardware { get; private set; }

        public TWP_1AI(ushort[] registradores)
        {
            NumeroSerie = (long)(registradores[0] << 16 | registradores[1]);

            Modelo = IdentificarModelo(registradores[2]);

            RSSI = registradores[3] / -2;

            PeriodoComunicacao = registradores[4];
            TempoDecorrido = registradores[5];

            TensaoAlimentacao = registradores[6] / 10.0f;

            TemperaturaInterna = (float)
                Math.Round(Conversor.ToFloat(registradores[8], registradores[7]), 2);

            ValorEntradaAnalogica_1 = (float)
                Math.Round(Conversor.ToFloat(registradores[10], registradores[9]), 2);

            VersaoFirmware = registradores[17];
            RevisaoVersao = registradores[18];
            VersaoHardware = registradores[19];
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
                        Tipo = "temperatura-interna",
                        Valor = TemperaturaInterna,
                        Unidade = "°C",
                    },
                    new Metrica
                    {
                        Tipo = "tensao-alimentacao",
                        Valor = TensaoAlimentacao,
                        Unidade = "V",
                    },
                    new Metrica
                    {
                        Tipo = "rssi",
                        Valor = RSSI,
                        Unidade = "dBm",
                    },
                ],
                Metadados = new Metadados { Modelo = Modelo, VersaoFirmware = VersaoFirmware },
            };
        }

        private static string IdentificarModelo(int modelo)
        {
            return modelo switch
            {
                47 => "TWP-1AI 868 MHZ",
                53 => "TWP-1AI 915 MHZ",
                _ => "",
            };
        }
    }
}
