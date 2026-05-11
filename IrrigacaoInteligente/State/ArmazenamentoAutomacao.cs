using System.Text.Json.Serialization;

namespace IrrigacaoInteligente.State
{
    public class ArmazenamentoAutomacao
    {
        public List<Controlador> Controladores { get; set; } = [];

        public bool Invalido => Controladores.Count == 0;
    }

    public class Controlador
    {
        public Guid Id { get; set; }

        public bool Master { get; set; }

        public bool Arquivado { get; set; }

        public string? Estagio { get; set; }

        public string? Descricao { get; set; }

        public string? Marca { get; set; }

        public string? Modelo { get; set; }

        public ConexoesModel? Conexoes { get; set; }
    }

    public class ConexoesModel
    {
        public string? Host { get; set; }

        public List<PortaModel> Saidas { get; set; } = [];

        public List<PortaModel> Entradas { get; set; } = [];

        public List<InterfaceModel> Interfaces { get; set; } = [];
    }

    public class PortaModel
    {
        public Guid Id { get; set; }

        public string? Tipo { get; set; }

        public string? Sinal { get; set; }

        public string? Faixa { get; set; }

        public string? Status { get; set; }

        public string? Descricao { get; set; }

        public string? Endereco { get; set; }

        public DispositivoModel? Conectado { get; set; }
    }

    public class DispositivoModel
    {
        public Guid Id { get; set; }

        public bool Arquivado { get; set; }

        public bool Habilitado { get; set; }

        public string? Descricao { get; set; }

        public string? Tipo { get; set; }

        public string? Sinal { get; set; }

        public int Categoria { get; set; }

        public ParametrosModel? Parametros { get; set; }
    }

    public class ParametrosModel
    {
        public double? ValorMinimo { get; set; }

        public double? ValorMaximo { get; set; }

        public dynamic? UnidadeMedida { get; set; }

        public double? Precisao { get; set; }

        public double? TempoResposta { get; set; }

        public double? TemperaturaOperacaoMin { get; set; }

        public double? TemperaturaOperacaoMax { get; set; }
    }

    public class InterfaceModel
    {
        public Guid Id { get; set; }

        public dynamic? Tipo { get; set; }

        public string? Status { get; set; }

        public string? Nome { get; set; }

        public string? Borne { get; set; }

        public string? Endereco { get; set; }

        public InterfaceConectadoModel? Conectado { get; set; }
    }

    public class InterfaceConectadoModel
    {
        public List<ModuloModel> Modulos { get; set; } = [];
    }

    public class ModuloModel
    {
        public Guid Id { get; set; }

        public bool Arquivado { get; set; }

        public string? Descricao { get; set; }

        public bool Master { get; set; }

        public string? Marca { get; set; }

        public string? Modelo { get; set; }

        public string? Estagio { get; set; }

        public string? Protocolo { get; set; }
    }
}
