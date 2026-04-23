namespace WorkerService.Features.Shared.Abstractions;

public class Comando
{
    public string IdentificadorPorta { get; init; } = null!;
    public string TipoComando { get; init; } = null!;
    public ParametrosConfiguracao Parametros { get; init; } = null!;

    public class ParametrosConfiguracao
    {
        public int ValorMinimo { get; init; }
        public int ValorMaximo { get; init; }
    }
}
