namespace IrrigacaoInteligente.Domain.Entities;

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
            Tipo = Entities.Tipo.ANALOGICA,
            Sinal = Entities.Sinal.ENTRADA,
            Operacao = Entities.Operacao.READ,
            Porta = porta,
        };

    public static ComandoAnalogico SetarValor(string porta, int valor)
    {
        if (valor < 0)
            throw new ArgumentException("Valor não pode ser negativo");

        return new()
        {
            Tipo = Entities.Tipo.ANALOGICA,
            Sinal = Entities.Sinal.SAIDA,
            Operacao = Entities.Operacao.WRITE,
            Porta = porta,
            Valor = valor,
        };
    }
}
