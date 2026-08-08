using LiteDB;
using controlador = Toolbox.Industrial.Core.Communication.Api.Contracts.Controlador;

namespace Toolbox.Industrial.Core.Data;

public class Controlador : Entity<controlador>
{
    public static bool Master = false;
    public static Guid PainelId = Guid.Empty;
    public static Guid ControladorId = Guid.Empty;

    protected Controlador() { }

    public Controlador(Guid id, controlador controlador)
        : base(id, controlador) { }

    [BsonField("Controlador")]
    public override controlador Valor { get; protected set; } = default!;
}
