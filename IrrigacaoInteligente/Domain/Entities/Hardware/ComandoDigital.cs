namespace IrrigacaoInteligente.Domain.Entities;

public class ComandoDigital
{
    public string Tipo { get; private init; } = null!;
    public string Sinal { get; private init; } = null!;
    public string Operacao { get; private init; } = null!;
    public string Porta { get; private init; } = null!;
    public bool? Valor { get; private init; }

    public static ComandoDigital Acionar(string porta) =>
        new()
        {
            Tipo = Entities.Tipo.DIGITAL,
            Sinal = Entities.Sinal.SAIDA,
            Operacao = Entities.Operacao.WRITE,
            Porta = porta,
            Valor = true,
        };

    public static ComandoDigital Desligar(string porta) =>
        new()
        {
            Tipo = Entities.Tipo.DIGITAL,
            Sinal = Entities.Sinal.SAIDA,
            Operacao = Entities.Operacao.WRITE,
            Porta = porta,
            Valor = false,
        };

    public static ComandoDigital Ler(string porta) =>
        new()
        {
            Tipo = Entities.Tipo.DIGITAL,
            Sinal = Entities.Sinal.ENTRADA,
            Operacao = Entities.Operacao.READ,
            Porta = porta,
        };
}
