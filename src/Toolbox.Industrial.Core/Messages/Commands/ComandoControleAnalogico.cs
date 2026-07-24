namespace Toolbox.Automacao.Core.Messages;

public class ComandoControleAnalogico
{
    public string Porta { get; set; } = string.Empty;
    public double Valor { get; set; } // valor analógico (frequência, abertura, etc.)
}
