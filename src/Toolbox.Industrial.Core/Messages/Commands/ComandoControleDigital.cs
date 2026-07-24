namespace Toolbox.Industrial.Core.Messages;

public class ComandoControleDigital
{
    public string Porta { get; set; } = string.Empty;
    public bool Valor { get; set; } // true = ligar/abrir, false = desligar/fechar
}
