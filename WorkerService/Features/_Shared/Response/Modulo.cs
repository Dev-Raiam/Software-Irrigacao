using WorkerService.Domain.Enums;

namespace WorkerService.Features.Shared.Response;

public class Modulo
{
    public Guid Id { get; set; }

    //public Guid Guid { get; set; }
    public bool Arquivado { get; set; }
    public ModuloEstagio Estagio { get; set; }
    public string? Descricao { get; set; }
    public ModuloMarca Marca { get; set; }
    public ModuloModelo Modelo { get; set; }
    public bool Master { get; set; }

    //public string Tipo { get; set; } = string.Empty;
    //public string Categoria { get; set; } = string.Empty;
    //public string? Host { get; set; }
    public List<Porta> Portas { get; set; } = new();
}
