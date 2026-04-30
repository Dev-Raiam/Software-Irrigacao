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
