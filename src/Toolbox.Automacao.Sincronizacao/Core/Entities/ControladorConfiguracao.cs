namespace Toolbox.Automacao.Sincronizacao.Core.Entities;

public sealed class ControladorConfiguracao
{
    public Guid Id { get; private set; }
    public Controlador Controlador { get; private set; } = new();

    public ControladorConfiguracao(Controlador controlador)
    {
        Id = controlador.Id;
        Controlador = controlador;
    }
}
