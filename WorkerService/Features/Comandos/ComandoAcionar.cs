namespace WorkerService.Features.Comandos;

public class ComandoAcionar
{
    public string IdentificadorPorta { get; set; } = null!;
    public string TipoComando { get; set; } = null!;
    public ParametrosConfiguracao Parametros { get; set; } = null!;

    public class ParametrosConfiguracao
    {
        public int ValorMinimo { get; set; }
        public int ValorMaximo { get; set; }
    }
}
