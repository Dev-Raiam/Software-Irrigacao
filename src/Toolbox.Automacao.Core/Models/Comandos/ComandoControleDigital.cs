namespace Toolbox.Automacao.Core.Models.Comandos;

public class ComandoControleDigital
{
    public string Porta { get; set; } = string.Empty;
    public bool Valor { get; set; } // true = ligar/abrir, false = desligar/fechar
}
