using LiteDB;
using System.Text.Json.Serialization;
using controlador = Toolbox.Industrial.Core.Communication.Api.Contracts.Controlador;

namespace Toolbox.Industrial.Core.Data;

public class Controlador : Entity<Guid, controlador> 
{
    protected Controlador() { }

    public Controlador(Guid id, controlador controlador) : base(id, controlador)
    {
    }

    [BsonField("Controlador")]
    [JsonPropertyOrder(100)]
    public override controlador Valor { get; protected set; } = default!;
}
