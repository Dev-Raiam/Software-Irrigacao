using LiteDB;

namespace Toolbox.Automacao.Core.Models;

public class ControladorConfiguracao
{
    [BsonId]
    public Guid Id { get; private set; }
    public Controlador Controlador { get; private set; } = null!;

    public ControladorConfiguracao(Controlador controlador)
    {
        Id = controlador.Id;
        Controlador = controlador;
    }
}
