namespace Toolbox.Industrial.Driver.TekonBkp.Models
{
    public class Metrica
    {
        public string? Porta { get; set; }
        public string Tipo { get; set; } = null!;
        public object Valor { get; set; } = null!;
        public string Unidade { get; set; } = null!;
    }
}
