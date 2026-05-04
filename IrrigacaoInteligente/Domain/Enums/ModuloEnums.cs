using System.Runtime.Serialization;

namespace IrrigacaoInteligente.Domain.Enums;

public enum ModuloTipo : int
{
    [EnumMember(Value = "Controlador master")]
    ControladorMaster = 1,

    [EnumMember(Value = "Controlador slave")]
    ControladorSlave = 2,

    [EnumMember(Value = "Módulo auxiliar")]
    ModuloAuxiliar = 3,
}

public enum ModuloEstagio : int
{
    [EnumMember(Value = "Pendente")]
    Pendente = 0,

    [EnumMember(Value = "Montagem")]
    Montagem = 1,

    [EnumMember(Value = "Montagem concluída")]
    MontagemConcluida = 2,

    [EnumMember(Value = "Instalação")]
    Instalacao = 3,

    [EnumMember(Value = "Instalação concluída")]
    InstalacaoConcluida = 4,

    [EnumMember(Value = "Em operação")]
    EmOperacao = 5,
}

public enum ModuloCategoria : long
{
    [EnumMember(Value = "Controlador")]
    Controlador = 01,

    [EnumMember(Value = "Controlador PLC")]
    ControladorPLC = 0101,

    [EnumMember(Value = "Controlador RTU")]
    ControladorRTU = 0102,

    [EnumMember(Value = "Controlador Edge")]
    ControladorEdge = 0103,

    [EnumMember(Value = "Gateway")]
    Gateway = 02,

    [EnumMember(Value = "Gateway industrial")]
    GatewayIndustrial = 0201,

    [EnumMember(Value = "Módulo de comunicação")]
    ModuloComunicacao = 0202,

    [EnumMember(Value = "Conversor de protocolo")]
    ConversorProtocolo = 0203,

    [EnumMember(Value = "Virtual")]
    Virtual = 03,

    [EnumMember(Value = "Simulador")]
    Simulador = 0301,

    [EnumMember(Value = "Módulo de cálculo")]
    ModuloCalculo = 0302,

    [EnumMember(Value = "Dispositivo lógico")]
    DispositivoLogico = 0303,
}

public enum ModuloMarca : long
{
    [EnumMember(Value = "Industrial Shields")]
    IndustrialShields = 010101,

    [EnumMember(Value = "Tekon")]
    Tekon = 020101,

    [EnumMember(Value = "ModuleX")]
    ModuleX = 020102,
}

public enum ModuloModelo : long
{
    [EnumMember(Value = "Gateberry")]
    Gateberry = 01010101,

    [EnumMember(Value = "Raspberry PLC Ethernet CPU")]
    RaspberryPLC_CPU = 01010102,

    [EnumMember(Value = "Raspberry PLC 19R")]
    RaspberryPLC_19R = 01010103,

    [EnumMember(Value = "Raspberry PLC 21")]
    RaspberryPLC_21 = 01010104,

    [EnumMember(Value = "Raspberry PLC 38AR")]
    RaspberryPLC_38AR = 01010105,

    [EnumMember(Value = "Raspberry PLC 38R")]
    RaspberryPLC_38R = 01010106,

    [EnumMember(Value = "Raspberry PLC 42")]
    RaspberryPLC_42 = 01010107,

    [EnumMember(Value = "Raspberry PLC 50RRA")]
    RaspberryPLC_50RRA = 01010108,

    [EnumMember(Value = "Raspberry PLC 53ARR")]
    RaspberryPLC_53ARR = 01010109,

    [EnumMember(Value = "Raspberry PLC 54ARA")]
    RaspberryPLC_54ARA = 01010110,

    [EnumMember(Value = "Raspberry PLC 57AAR")]
    RaspberryPLC_57AAR = 01010111,

    [EnumMember(Value = "Raspberry PLC 57R")]
    RaspberryPLC_57R = 01010112,

    [EnumMember(Value = "Raspberry PLC 58")]
    RaspberryPLC_58 = 01010113,

    [EnumMember(Value = "Gateway WGW420")]
    Gateway_WGW420 = 02010101,

    [EnumMember(Value = "Repetidor WRP001")]
    Repeater_WRP001 = 0201010101,

    [EnumMember(Value = "Transmissor TWP-1AI")]
    Transmitter_TWP1AI = 0201010102,

    [EnumMember(Value = "Transmissor TWP-2AI")]
    Transmitter_TWP2AI = 0201010103,

    [EnumMember(Value = "Transmissor TWP4AI")]
    Transmitter_TWP4AI = 0201010104,

    [EnumMember(Value = "Transmissor TWP-4AI4DI1UT")]
    Transmitter_TWP4AI4DI1UT = 0201010105,

    [EnumMember(Value = "Transmissor TWP-1DI")]
    Transmitter_TWP1DI = 0201010106,

    [EnumMember(Value = "Transmissor TWP-2DI")]
    Transmitter_TWP2DI = 0201010107,

    [EnumMember(Value = "Transmissor TWP-1UT")]
    Transmitter_TWP1UT = 0201010108,

    [EnumMember(Value = "Transmissor TWP-2UT")]
    Transmitter_TWP2UT = 0201010109,

    [EnumMember(Value = "Transmissor TWPH-1UT")]
    Transmitter_TWPH1UT = 0201010110,

    [EnumMember(Value = "MX-master 485")]
    MXmaster485 = 02010201,

    [EnumMember(Value = "MX-4AI")]
    MX4AI = 0201020101,

    [EnumMember(Value = "MX-4AOV")]
    MX4AOV = 0201020102,

    [EnumMember(Value = "MX-8DI")]
    MX8DI = 0201020103,

    [EnumMember(Value = "MX-8DO")]
    MX8DO = 0201020104,
}
