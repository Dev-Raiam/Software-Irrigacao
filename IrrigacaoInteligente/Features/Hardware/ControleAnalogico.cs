namespace IrrigacaoInteligente.Features.Hardware;

public class ControleAnalogico
{
    public string Tipo { get; private init; } = null!;
    public string Sinal { get; private init; } = null!;
    public string Operacao { get; private init; } = null!;
    public string Porta { get; private init; } = null!;
    public int? Valor { get; private set; }

    public static ControleAnalogico Ler(string porta) =>
        new()
        {
            Tipo = "analogico",
            Sinal = Hardware.Sinal.ENTRADA,
            Operacao = Hardware.Operacao.READ,
            Porta = porta,
        };

    public ControleAnalogico SetarValor(string porta, int valor)
    {
        if (valor < 0)
            throw new ArgumentException("Valor não pode ser negativo");

        return new()
        {
            Tipo = "analogico",
            Sinal = Hardware.Sinal.SAIDA,
            Operacao = Hardware.Operacao.WRITE,
            Porta = porta,
            Valor = valor,
        };
    }
}
