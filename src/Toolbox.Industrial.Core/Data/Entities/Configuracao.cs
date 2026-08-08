using LiteDB;
using Microsoft.AspNetCore.Http.Timeouts;

namespace Toolbox.Industrial.Core.Data;

public class Configuracao : Entity<object>
{
    protected Configuracao() { }

    public Configuracao(Guid id, object configuracao, grupo grupo, tipo tipo)
        : base(id, configuracao)
    {
        Grupo = (int)grupo;
        Tipo = (int)tipo;
    }

    public int Grupo { get; protected set; } = default!;

    public int Tipo { get; protected set; } = default!;

    [BsonField("Configuracao")]
    public override object Valor { get; protected set; } = default!;

    public enum grupo : int
    {
        Indefinido = 0,
        Api = 1,
        App = 2,
        Log = 3,
        Auth = 4,
        Mqtt = 5,
    }

    public enum tipo : int
    {
        Indefinido = 0,
        Seguranca = 1,
        Config = 2,
        Topico = 3,
    }
}
