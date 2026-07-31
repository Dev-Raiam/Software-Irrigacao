using LiteDB;
using controlador = Toolbox.Industrial.Core.Communication.Api.Contracts.Controlador;

namespace Toolbox.Industrial.Core.Data;

public class Controlador : Entity<Guid, controlador>
{
    public static bool Master = false;

    protected Controlador() { }

    public Controlador(Guid id, controlador controlador)
        : base(id, controlador) { }

    [BsonField("Controlador")]
    public override controlador Valor { get; protected set; } = default!;
}
