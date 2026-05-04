using System.Runtime.Serialization;

namespace IrrigacaoInteligente.Domain.Enums;

public enum PortaTipo : int
{
    [EnumMember(Value = "Não se aplica")]
    NaoSeAplica = 0,

    [EnumMember(Value = "Entrada")]
    Entrada = 1,

    [EnumMember(Value = "Saida")]
    Saida = 2,
}

public enum PortaSinal : int
{
    [EnumMember(Value = "Não se aplica")]
    NaoSeAplica = 0,

    [EnumMember(Value = "Sinal digital")]
    Digital = 1,

    [EnumMember(Value = "Sinal analógico")]
    Analogico = 2,

    [EnumMember(Value = "Sinal temperatura")]
    Temperatura = 3,

    [EnumMember(Value = "Sinal configurável")]
    Configuravel = 9,
}

public enum PortaCategoria : int
{
    [EnumMember(Value = "Interface")]
    Interface = 0,

    [EnumMember(Value = "Entrada")]
    Entrada = 1,

    [EnumMember(Value = "Saida")]
    Saida = 2,
}

public enum PortaStatus : int
{
    [EnumMember(Value = "Desabilitada")]
    Desabilitada = 0,

    [EnumMember(Value = "Habilitada")]
    Habilitada = 1,

    [EnumMember(Value = "Inutilizada")]
    Inutilizada = 2,
}
