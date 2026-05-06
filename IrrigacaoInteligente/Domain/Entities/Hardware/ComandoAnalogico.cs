namespace IrrigacaoInteligente.Domain.Entities.Hardware;

public class ComandoAnalogico
{
    public Guid CorrelacaoId { get; private init; }
    public string Tipo { get; private init; } = null!;
    public string Sinal { get; private init; } = null!;
    public string Operacao { get; private init; } = null!;
    public string Porta { get; private init; } = null!;
    public int? Valor { get; private set; }

    public static ComandoAnalogico Ler(string porta) =>
        new()
        {
            Tipo = Hardware.Tipo.ANALOGICA,
            Sinal = Hardware.Sinal.ENTRADA,
            Operacao = Hardware.Operacao.READ,
            Porta = porta,
        };

    public static ComandoAnalogico SetarValor(string porta, int valor)
    {
        if (valor < 0)
            throw new ArgumentException("Valor não pode ser negativo");

        return new()
        {
            Tipo = Hardware.Tipo.ANALOGICA,
            Sinal = Hardware.Sinal.SAIDA,
            Operacao = Hardware.Operacao.WRITE,
            Porta = porta,
            Valor = valor,
        };
    }
}
