namespace IrrigacaoInteligente.Features.Shared.Response;

public class Painel
{
    public Guid Id { get; set; }
    public bool Arquivado { get; set; }
    public bool Primario { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public string Referencia { get; set; } = null!;
    public List<Modulo> Controladores { get; set; } = [];
    public List<Modulo> Modulos { get; set; } = [];
}
