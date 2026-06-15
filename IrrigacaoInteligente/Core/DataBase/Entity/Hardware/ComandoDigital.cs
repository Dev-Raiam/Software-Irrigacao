//namespace IrrigacaoInteligente.Core.DataBase.Entity;

//public class ComandoDigital
//{
//    public string Tipo { get; private init; } = null!;
//    public string Sinal { get; private init; } = null!;
//    public string Operacao { get; private init; } = null!;
//    public string Porta { get; private init; } = null!;
//    public bool? Valor { get; private init; }

//    public static ComandoDigital Acionar(string porta) =>
//        new()
//        {
//            Tipo = Entity.Tipo.DIGITAL,
//            Sinal = Entity.Sinal.SAIDA,
//            Operacao = Entity.Operacao.WRITE,
//            Porta = porta,
//            Valor = true,
//        };

//    public static ComandoDigital Desligar(string porta) =>
//        new()
//        {
//            Tipo = Entity.Tipo.DIGITAL,
//            Sinal = Entity.Sinal.SAIDA,
//            Operacao = Entity.Operacao.WRITE,
//            Porta = porta,
//            Valor = false,
//        };

//    public static ComandoDigital Ler(string porta) =>
//        new()
//        {
//            Tipo = Entity.Tipo.DIGITAL,
//            Sinal = Entity.Sinal.ENTRADA,
//            Operacao = Entity.Operacao.READ,
//            Porta = porta,
//        };
//}
