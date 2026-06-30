using System;
using System.Collections.Generic;
using System.Text;

namespace Toolbox.Automacao.Irrigacao.Models
{
    public class Telemetria
    {
        public Guid Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string? Status { get; set; } = null!;
        public List<Metrica> Metricas { get; set; } = [];
        public Metadados Metadados { get; set; } = null!;
    }

    public class Metrica
    {
        public string Tipo { get; set; } = null!;
        public object Valor { get; set; } = null!;
        public string Unidade { get; set; } = null!;
    }

    public class Metadados
    {
        public string Modelo { get; set; } = null!;
        public int? VersaoFirmware { get; set; }
    }
}
