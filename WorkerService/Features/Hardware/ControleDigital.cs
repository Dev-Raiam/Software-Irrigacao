namespace WorkerService.Features.Hardware;

public class ControleDigital
{
    public string Tipo { get; private init; } = null!;
    public string Sinal { get; private init; } = null!;
    public string Operacao { get; private init; } = null!;
    public string Porta { get; private init; } = null!;
    public bool? Valor { get; private init; }

    public static ControleDigital Acionar(string porta) =>
        new()
        {
            Tipo = "digital",
            Sinal = Hardware.Sinal.SAIDA,
            Operacao = Hardware.Operacao.WRITE,
            Porta = porta,
            Valor = true,
        };

    public static ControleDigital Desligar(string porta) =>
        new()
        {
            Tipo = "digital",
            Sinal = Hardware.Sinal.SAIDA,
            Operacao = Hardware.Operacao.WRITE,
            Porta = porta,
            Valor = false,
        };

    public static ControleDigital Ler(string porta) =>
        new()
        {
            Tipo = "digital",
            Sinal = Hardware.Sinal.ENTRADA,
            Operacao = Hardware.Operacao.READ,
            Porta = porta,
        };
}
