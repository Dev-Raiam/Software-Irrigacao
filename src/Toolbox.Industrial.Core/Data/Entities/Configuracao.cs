using LiteDB;
using System.Text.Json.Serialization;

namespace Toolbox.Industrial.Core.Data;

public class Configuracao : Entity<Guid, object> 
{
    protected Configuracao() { }

    public Configuracao(Guid id, object configuracao, Tipo tipo = Tipo.Indefinido) : base(id, configuracao)
    {
        Type = tipo;
    }

    [BsonField("Tipo")]
    public Tipo Type { get; protected set; } = default!;

    [BsonField("Configuracao")]
    public override object Valor { get; protected set; } = default!;

    public enum Tipo : int 
    {
        Indefinido = 0,
    }
}
