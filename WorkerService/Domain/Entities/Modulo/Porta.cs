using WorkerService.Domain.Enums;

namespace WorkerService.Domain.Entities;

public class Porta
{
    protected Porta() { }

    public Porta(
        Guid id,
        Guid moduloId,
        Guid? dispositivoConectadoId,
        PortaTipo tipo,
        PortaSinal sinal,
        PortaStatus status,
        string nome,
        string? enderecoBorne,
        string? enderecoLogico
    )
    {
        Id = id;
        ModuloId = moduloId;
        DispositivoConectadoId = dispositivoConectadoId;
        Tipo = tipo;
        Sinal = sinal;
        Status = status;
        Nome = nome;
        EnderecoBorne = enderecoBorne;
        EnderecoLogico = enderecoLogico;
    }

    public Guid Id { get; private set; }
    public Guid ModuloId { get; private set; }
    public Modulo Modulo { get; private set; } = null!;
    public Guid? DispositivoConectadoId { get; private set; }
    public Dispositivo? DispositivoConectado { get; private set; }
    public PortaTipo Tipo { get; private set; }
    public PortaSinal Sinal { get; private set; }
    public PortaStatus Status { get; private set; }
    public string Nome { get; private set; } = null!;
    public string? EnderecoBorne { get; private set; }
    public string? EnderecoLogico { get; private set; }

    public Porta Atualizar(
        Guid? dispositivoId,
        PortaTipo tipo,
        PortaSinal sinal,
        PortaStatus status,
        string nome,
        string? enderecoBorne,
        string? enderecoLogico
    )
    {
        DispositivoConectadoId = dispositivoId;
        Tipo = tipo;
        Sinal = sinal;
        Status = status;
        Nome = nome;
        EnderecoBorne = enderecoBorne;
        EnderecoLogico = enderecoLogico;
        return this;
    }
}
