namespace Toolbox.Automacao.Irrigacao.Marcas.EletronicaSanterno.Modelos
{
    public class SinusM
    {
        // 0x0000
        public string ModeloInversor { get; private set; } = null!;

        // 0x0001
        public string CapacidadeInversor { get; private set; } = null!;

        // 0x0002
        public string TensaoEntrada { get; private set; } = null!;

        // 0x0003
        public string VersaoSoftware { get; private set; } = null!;

        // 0x0005
        public double FrequenciaReferencia { get; private set; }

        // 0x0007
        public double TempoAceleracao { get; private set; }

        // 0x0008
        public double TempoDesaceleracao { get; private set; }

        // 0x0009
        public double CorrenteSaida { get; private set; }

        // 0x000A
        public double FrequenciaSaida { get; private set; }

        // 0x000B
        public double TensaoSaida { get; private set; }

        // 0x000C
        public double TensaoBarramentoCC { get; private set; }

        // 0x000D
        public double PotenciaSaida { get; private set; }

        // 0x000E
        public EstadoInversor Estado { get; private set; }

        // 0x000F
        public InfoIntervencao Intervencao { get; private set; }

        // 0x0010
        public EstadoEntradasDigitais EntradasDigitais { get; private set; }

        // 0x0011
        public EstadoSaidasDigitais SaidasDigitais { get; private set; }

        // 0x0012
        public double EntradaAnalogicoV1 { get; private set; }

        // 0x0013
        public double EntradaAnalogicoV2 { get; private set; }

        // 0x0014
        public double EntradaAnalogicoI { get; private set; }

        // 0x0015
        public int RPM { get; private set; }

        /// <summary>
        /// Construtor esperando registradores lidos a partir do endereço 0x0000
        /// até 0x0015 (22 registradores no total).
        /// </summary>
        public SinusM(ushort[] registradores)
        {
            ModeloInversor = IdentificarModelo(registradores[0x0000]);
            CapacidadeInversor = IdentificarCapacidade(registradores[0x0001]);
            TensaoEntrada = IdentificarTensaoEntrada(registradores[0x0002]);
            VersaoSoftware = IdentificarVersaoSoftware(registradores[0x0003]);

            // 0x0004 = bloqueio de parâmetros — ignorado aqui (somente configuração)

            FrequenciaReferencia = registradores[0x0005] / 100.0; // escala 0.01 Hz

            // 0x0006 = comando de marcha — ignorado aqui (somente comando)

            TempoAceleracao = registradores[0x0007] / 10.0; // escala 0.1 seg
            TempoDesaceleracao = registradores[0x0008] / 10.0; // escala 0.1 seg
            CorrenteSaida = registradores[0x0009] / 10.0; // escala 0.1 A
            FrequenciaSaida = registradores[0x000A] / 100.0; // escala 0.01 Hz
            TensaoSaida = registradores[0x000B] / 10.0; // escala 0.1 V
            TensaoBarramentoCC = registradores[0x000C] / 10.0; // escala 0.1 V
            PotenciaSaida = registradores[0x000D] / 10.0; // escala 0.1 kW

            Estado = new EstadoInversor(registradores[0x000E]);
            Intervencao = new InfoIntervencao(registradores[0x000F]);
            EntradasDigitais = new EstadoEntradasDigitais(registradores[0x0010]);
            SaidasDigitais = new EstadoSaidasDigitais(registradores[0x0011]);

            EntradaAnalogicoV1 = registradores[0x0012] / (double)0x3FFF * 10.0; // 0V ~ +10V
            EntradaAnalogicoV2 = registradores[0x0013] / (double)0x3FFF * 10.0; // 0V ~ -10V
            EntradaAnalogicoI = registradores[0x0014] / (double)0x3FFF * 20.0; // 0 ~ 20mA

            RPM = registradores[0x0015];
        }

        private static string IdentificarModelo(ushort valor) =>
            valor switch
            {
                0xA => "SINUS M",
                0x7 => "VEGA DRIVE",
                0x8 => "SINUS N / ORION DRIVE",
                _ => $"Desconhecido (0x{valor:X4})",
            };

        private static string IdentificarCapacidade(ushort valor) =>
            valor switch
            {
                0xFFFF => "0.4 kW",
                0x0000 => "0.75 kW",
                0x0002 => "1.5 kW",
                0x0003 => "2.2 kW",
                0x0004 => "3.7 kW",
                0x0005 => "4.0 kW",
                0x0006 => "5.5 kW",
                0x0007 => "7.5 kW",
                _ => $"Desconhecido (0x{valor:X4})",
            };

        private static string IdentificarTensaoEntrada(ushort valor) =>
            valor switch
            {
                0 => "Classe 2S/T (200-230V)",
                1 => "Classe 4T (380-480V)",
                _ => $"Desconhecido (0x{valor:X4})",
            };

        private static string IdentificarVersaoSoftware(ushort valor)
        {
            int maior = (valor >> 8) & 0xFF;
            int menor = valor & 0xFF;
            return $"V{maior}.{menor}";
        }
    }

    /// <summary>
    /// Decodifica o registrador 0x000E — Estado do inversor (word de bits).
    /// </summary>
    public class EstadoInversor
    {
        public bool Stop { get; } // BIT 0
        public bool MarchaAFrente { get; } // BIT 1
        public bool MarchaReversa { get; } // BIT 2
        public bool Avaria { get; } // BIT 3
        public bool Acelerando { get; } // BIT 4
        public bool Desacelerando { get; } // BIT 5
        public bool VelocidadeAlcancada { get; } // BIT 6
        public bool FrenagemCC { get; } // BIT 7
        public bool Parado { get; } // BIT 8
        public bool FrenagemAberta { get; } // BIT 10
        public bool CmdMarchaAFrente { get; } // BIT 11
        public bool CmdMarchaReversa { get; } // BIT 12
        public bool ReferenciaRemotaRS { get; } // BIT 13
        public bool ReferenciaRemotaFreq { get; } // BIT 14

        public EstadoInversor(ushort valor)
        {
            Stop = (valor & (1 << 0)) != 0;
            MarchaAFrente = (valor & (1 << 1)) != 0;
            MarchaReversa = (valor & (1 << 2)) != 0;
            Avaria = (valor & (1 << 3)) != 0;
            Acelerando = (valor & (1 << 4)) != 0;
            Desacelerando = (valor & (1 << 5)) != 0;
            VelocidadeAlcancada = (valor & (1 << 6)) != 0;
            FrenagemCC = (valor & (1 << 7)) != 0;
            Parado = (valor & (1 << 8)) != 0;
            FrenagemAberta = (valor & (1 << 10)) != 0;
            CmdMarchaAFrente = (valor & (1 << 11)) != 0;
            CmdMarchaReversa = (valor & (1 << 12)) != 0;
            ReferenciaRemotaRS = (valor & (1 << 13)) != 0;
            ReferenciaRemotaFreq = (valor & (1 << 14)) != 0;
        }
    }

    /// <summary>
    /// Decodifica o registrador 0x000F — Informações de intervenção/falha (word de bits).
    /// </summary>
    public class InfoIntervencao
    {
        public bool OCT { get; } // BIT 0  - Sobrecorrente
        public bool OVT { get; } // BIT 1  - Sobretensão
        public bool EXTA { get; } // BIT 2  - Falha externa A
        public bool BX { get; } // BIT 3  - Bloqueio (EST)
        public bool COL { get; } // BIT 4
        public bool GFT { get; } // BIT 5  - Falha de aterramento
        public bool OHT { get; } // BIT 6  - Superaquecimento inversor
        public bool ETH { get; } // BIT 7  - Superaquecimento motor
        public bool OLT { get; } // BIT 8  - Sobrecarga
        public bool HWDiag { get; } // BIT 9
        public bool EXTB { get; } // BIT 10 - Falha externa B
        public bool EEP { get; } // BIT 11 - Erro escrita EEPROM
        public bool FAN { get; } // BIT 12 - Erro ventilador
        public bool PO { get; } // BIT 13 - Fase aberta
        public bool IOLT { get; } // BIT 14
        public bool LVT { get; } // BIT 15 - Subtensão

        public InfoIntervencao(ushort valor)
        {
            OCT = (valor & (1 << 0)) != 0;
            OVT = (valor & (1 << 1)) != 0;
            EXTA = (valor & (1 << 2)) != 0;
            BX = (valor & (1 << 3)) != 0;
            COL = (valor & (1 << 4)) != 0;
            GFT = (valor & (1 << 5)) != 0;
            OHT = (valor & (1 << 6)) != 0;
            ETH = (valor & (1 << 7)) != 0;
            OLT = (valor & (1 << 8)) != 0;
            HWDiag = (valor & (1 << 9)) != 0;
            EXTB = (valor & (1 << 10)) != 0;
            EEP = (valor & (1 << 11)) != 0;
            FAN = (valor & (1 << 12)) != 0;
            PO = (valor & (1 << 13)) != 0;
            IOLT = (valor & (1 << 14)) != 0;
            LVT = (valor & (1 << 15)) != 0;
        }
    }

    /// <summary>
    /// Decodifica o registrador 0x0010 — Estado das entradas digitais.
    /// </summary>
    public class EstadoEntradasDigitais
    {
        public bool P1 { get; } // BIT 0
        public bool P2 { get; } // BIT 1
        public bool P3 { get; } // BIT 2
        public bool P4 { get; } // BIT 3
        public bool P5 { get; } // BIT 4
        public bool P6 { get; } // BIT 5
        public bool P7 { get; } // BIT 6
        public bool P8 { get; } // BIT 7

        public EstadoEntradasDigitais(ushort valor)
        {
            P1 = (valor & (1 << 0)) != 0;
            P2 = (valor & (1 << 1)) != 0;
            P3 = (valor & (1 << 2)) != 0;
            P4 = (valor & (1 << 3)) != 0;
            P5 = (valor & (1 << 4)) != 0;
            P6 = (valor & (1 << 5)) != 0;
            P7 = (valor & (1 << 6)) != 0;
            P8 = (valor & (1 << 7)) != 0;
        }
    }

    /// <summary>
    /// Decodifica o registrador 0x0011 — Estado das saídas digitais.
    /// </summary>
    public class EstadoSaidasDigitais
    {
        public bool MO { get; } // BIT 4 - Multi-saída com OC
        public bool ABC { get; } // BIT 7 - Relé 3ABC

        public EstadoSaidasDigitais(ushort valor)
        {
            MO = (valor & (1 << 4)) != 0;
            ABC = (valor & (1 << 7)) != 0;
        }
    }
}
