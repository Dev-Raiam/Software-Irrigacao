using System.Runtime.Serialization;

namespace WorkerService.Domain.Enums;

public enum DispositivoTipo : long
{
    [EnumMember(Value = "Sensor de tensão")]
    SensorTensao = 050101,

    [EnumMember(Value = "Sensor de corrente")]
    SensorCorrente = 050102,

    [EnumMember(Value = "Sensor de potência")]
    SensorPotencia = 050103,

    [EnumMember(Value = "Sensor de frequência")]
    SensorFrequencia = 050104,

    [EnumMember(Value = "Sensor de nível")]
    SensorNivel = 050201,

    [EnumMember(Value = "Sensor de pressão")]
    SensorPressao = 050202,

    [EnumMember(Value = "Sensor de boia")]
    SensorBoia = 050301,

    [EnumMember(Value = "Sensor de posição")]
    SensorPosicao = 050302,

    [EnumMember(Value = "Válvula solenoide")]
    ValvulaSolenoide = 0600,

    [EnumMember(Value = "Comando de partida")]
    ComandoPartida = 060401,

    [EnumMember(Value = "Comando de ativação")]
    ComandoAtivacao = 060402,

    [EnumMember(Value = "Comando de abertura")]
    ComandoAbertura = 060403,

    [EnumMember(Value = "Comando de fechamento")]
    ComandoFechamento = 060404,

    [EnumMember(Value = "Comando de velocidade")]
    ComandoVelocidade = 060405,

    [EnumMember(Value = "Comando de retrolavagem")]
    ComandoRetrolavagem = 060406,
}

public enum DispositivoUnidadeMedida : int
{
    [EnumMember(Value = "KPa")]
    KPa = 05020201,

    [EnumMember(Value = "PSI")]
    PSI = 05020202,

    [EnumMember(Value = "Bar")]
    Bar = 05020203,

    [EnumMember(Value = "MPa")]
    MPa = 05020204,

    [EnumMember(Value = "Celsius")]
    Celsius = 5,

    [EnumMember(Value = "Kelvin")]
    Kelvin = 6,

    [EnumMember(Value = "Fahrenheit")]
    Fahrenheit = 7,

    [EnumMember(Value = "Metro")]
    Metro = 8,

    [EnumMember(Value = "Centimetro")]
    Centimetro = 9,

    [EnumMember(Value = "Percentual")]
    Percentual = 10,

    [EnumMember(Value = "Volt")]
    Volt = 11,

    [EnumMember(Value = "Ampere")]
    Ampere = 12,
}

// public enum DispositivoCategoria : long
// {
//     [EnumMember(Value = "Controlador")]
//     Controlador = 01,

//     [EnumMember(Value = "Controlador CLP")]
//     ControladorCLP = 0101,

//     [EnumMember(Value = "Controlador RTU")]
//     ControladorRTU = 0102,

//     [EnumMember(Value = "Controlador Edge")]
//     ControladorEdge = 0103,

//     [EnumMember(Value = "Gateway")]
//     Gateway = 02,

//     [EnumMember(Value = "Gateway industrial")]
//     GatewayIndustrial = 0201,

//     [EnumMember(Value = "Módulo de comunicação")]
//     ModuloComunicacao = 0202,

//     [EnumMember(Value = "Conversor de protocolo")]
//     ConversorProtocolo = 0203,

//     [EnumMember(Value = "Virtual")]
//     Virtual = 03,

//     [EnumMember(Value = "Simulador")]
//     Simulador = 0301,

//     [EnumMember(Value = "Módulo de cálculo")]
//     ModuloCalculo = 0302,

//     [EnumMember(Value = "Dispositivo lógico")]
//     DispositivoLogico = 0303,

//     [EnumMember(Value = "IHM")]
//     IHM = 04,

//     [EnumMember(Value = "Painel de operação")]
//     PainelOperacao = 0401,

//     [EnumMember(Value = "Sensor")]
//     Sensor = 05,

//     [EnumMember(Value = "Sensor elétrico")]
//     SensorEletrico = 0501,

//     [EnumMember(Value = "Sensor de processo")]
//     SensorProcesso = 0502,

//     [EnumMember(Value = "Sensor discreto")]
//     SensorDiscreto = 0503,

//     [EnumMember(Value = "Sensor ambiental")]
//     SensorAmbiental = 0504,

//     [EnumMember(Value = "Atuador")]
//     Atuador = 06,

//     [EnumMember(Value = "Atuador elétrico")]
//     AtuadorEletrico = 0601,

//     [EnumMember(Value = "Atuador pneumático")]
//     AtuadorPneumatico = 0602,

//     [EnumMember(Value = "Atuador hidráulico")]
//     AtuadorHidraulico = 0603,

//     [EnumMember(Value = "Atuador de comando lógico")]
//     AtuadorComandoLogico = 0604,

//     [EnumMember(Value = "Medidor")]
//     Medidor = 07,

//     [EnumMember(Value = "Medidor de fluxo")]
//     MedidorFluxo = 0701,

//     [EnumMember(Value = "Medidor de energia")]
//     MedidorEnergia = 0702,

//     [EnumMember(Value = "Indicador")]
//     Indicador = 08,

//     [EnumMember(Value = "Indicador de posição")]
//     IndicadorPosicao = 0801,

//     [EnumMember(Value = "Indicador de processo")]
//     IndicadorProcesso = 0802,

//     [EnumMember(Value = "Indicador sinalizador")]
//     IndicadorSinalizador = 0803,

//     [EnumMember(Value = "Analisador")]
//     Analisador = 09,

//     [EnumMember(Value = "Analisador químico")]
//     AnalisadorQuimico = 0901,

//     [EnumMember(Value = "Analisador de qualidade")]
//     AnalisadorQualidade = 0902,

//     [EnumMember(Value = "Segurança")]
//     Seguranca = 10,

//     [EnumMember(Value = "Relé de segurança")]
//     ReleSeguranca = 1001,

//     [EnumMember(Value = "Chave de emergência")]
//     ChaveEmergencia = 1002,

//     [EnumMember(Value = "Barreira de segurança")]
//     BarreiraSeguranca = 1003,

//     [EnumMember(Value = "Infraestrutura")]
//     Infraestrutura = 11,

//     [EnumMember(Value = "Quadro de comando")]
//     QuadroComando = 1101,

//     [EnumMember(Value = "Switch industrial")]
//     SwitchIndustrial = 1102,

//     [EnumMember(Value = "Fonte de alimentação")]
//     FonteAlimentacao = 1103,
// }
