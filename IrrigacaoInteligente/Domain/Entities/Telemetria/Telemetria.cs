namespace IrrigacaoInteligente.Domain.Entities;

public class Telemetria
{
    public Guid Id { get; private set; }
    public Guid ControladorId { get; private set; }
    public Guid DispositivoId { get; private set; }
    public string Descricao { get; private set; }
    public DateTime CriadoEm { get; private set; }
    public dynamic? Dados { get; private set; }

    public Telemetria(Guid controladorId, Guid dispositivoId, string descricao, string? dados)
    {
        Id = Guid.NewGuid();
        ControladorId = controladorId;
        DispositivoId = dispositivoId;
        Descricao = descricao;
        CriadoEm = DateTime.UtcNow;
        Dados = dados;
    }
}
