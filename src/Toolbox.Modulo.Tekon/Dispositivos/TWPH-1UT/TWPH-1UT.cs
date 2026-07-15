//namespace Toolbox.Modulo.Tekon.Modelos
//{
//    public class TWPH_1UT
//    {
//        public long NumeroSerie { get; private set; }
//        public string Modelo { get; set; } = null!;
//        public int RSSI { get; private set; }
//        public int PeriodoComunicacao { get; private set; }
//        public int TempoDecorrido { get; private set; }
//        public float TensaoAlimentacao { get; private set; }
//        public float TemperaturaInterna { get; private set; }
//        public float TemperaturaExterna { get; private set; }
//        public int VersaoFirmware { get; private set; }
//        public int RevisaoVersao { get; private set; }
//        public int VersaoHardware { get; private set; }

//        public TWPH_1UT(ushort[] buffer)
//        {
//            NumeroSerie = (long)(buffer[0] << 16 | buffer[1]);

//            Modelo = IdentificarModelo(buffer[2]);

//            RSSI = buffer[3] / -2;

//            PeriodoComunicacao = buffer[4];
//            TempoDecorrido = buffer[5];

//            TensaoAlimentacao = buffer[6] / 10;

//            TemperaturaInterna = (float)Math.Round(Conversor.ToFloat(buffer[8], buffer[7]), 2);
//            TemperaturaExterna = (float)Math.Round(Conversor.ToFloat(buffer[10], buffer[9]), 2);

//            VersaoFirmware = buffer[17];
//            RevisaoVersao = buffer[18];
//            VersaoHardware = buffer[19];
//        }

//        public Telemetria ObterTelemetria(Guid moduloId)
//        {
//            return new Telemetria
//            {
//                Id = moduloId,
//                Timestamp = DateTime.Now,
//                Status = (TempoDecorrido > 10) ? "offline" : "online",
//                Metricas =
//                [
//                    new Metrica
//                    {
//                        Tipo = "temperatura-interna",
//                        Valor = TemperaturaInterna,
//                        Unidade = "°C",
//                    },
//                    new Metrica
//                    {
//                        Tipo = "temperatura-externa",
//                        Valor = TemperaturaExterna,
//                        Unidade = "°C",
//                    },
//                    new Metrica
//                    {
//                        Tipo = "tensao-alimentacao",
//                        Valor = TensaoAlimentacao,
//                        Unidade = "V",
//                    },
//                    new Metrica
//                    {
//                        Tipo = "rssi",
//                        Valor = RSSI,
//                        Unidade = "dBm",
//                    },
//                ],
//                Metadados = new Metadados { Modelo = Modelo, VersaoFirmware = VersaoFirmware },
//            };
//        }

//        private static string IdentificarModelo(int modelo)
//        {
//            return modelo switch
//            {
//                24 => "TWPH-1UT 868 MHZ",
//                28 => "TWPH-1UT 915 MHZ",
//                _ => "",
//            };
//        }
//    }
//}
