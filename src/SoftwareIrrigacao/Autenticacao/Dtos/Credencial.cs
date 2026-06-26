namespace Autenticacao.Dtos;

public class Credencial
{
    public string Chave { get; init; } = null!;
    public string Segredo { get; init; } = null!;
    public Guid ContextoId { get; init; }
}
