namespace IrrigacaoInteligente.Domain.Entities;

public static class Operacao
{
    public static string WRITE { get; } = "write";
    public static string READ { get; } = "read";
}

public static class Sinal
{
    public static string ENTRADA { get; } = "entrada";
    public static string SAIDA { get; } = "saida";
}

public static class Tipo
{
    public static string DIGITAL { get; } = "digital";
    public static string ANALOGICA { get; } = "analogica";
}
