using LiteDB;

namespace Toolbox.Industrial.Core.Data;

public class Telemetria : Entity<Guid, object>
{
    protected Telemetria() { }

    public Telemetria(Guid id, object telemetria, tipo tipo)
        : base(id, telemetria)
    {
        Tipo = (int)tipo;
        Status = (int)status.Pendente;
    }

    public int Tipo { get; protected set; } = default!;

    public int Status { get; protected set; } = default!;

    [BsonField("Telemetria")]
    public override object Valor { get; protected set; } = default!;

    public virtual void Atualizar(status status)
    {
        Status = (int)status;
        UltimaAtualizacao = DateTime.UtcNow;
    }

    public enum tipo : int
    {
        Controlador = 0,
        Dispositivo = 1,
    }

    public enum status : int
    {
        Pendente = 0,
    }
}
