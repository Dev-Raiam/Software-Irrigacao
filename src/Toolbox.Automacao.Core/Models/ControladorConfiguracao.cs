namespace Toolbox.Automacao.Core.Models;

public class ControladorConfiguracao
{
    public Guid Id { get; private set; }
    public Controlador Controlador { get; private set; } = null!;

    protected ControladorConfiguracao() { }

    public ControladorConfiguracao(Controlador controlador)
    {
        Id = controlador.Id;
        Controlador = controlador;
    }
}
