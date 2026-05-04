using IrrigacaoInteligente.Domain.Entities;
using IrrigacaoInteligente.Domain.Enums;

namespace IrrigacaoInteligente.Features.Shared.Response
{
    public class Interface
    {
        public Guid Id { get; init; }
        public InterfaceTipo Tipo { get; init; }
        public string Porta { get; init; } = null!;
        public PortaStatus Status { get; init; }
        public ModuloMarca Marca { get; init; }
        public ModuloModelo Modelo { get; init; }
        public ModuloCategoria Categoria { get; init; }
        public Conexao? Conectado { get; init; }
        public ModbusResponse? Modbus { get; init; }
        public string? EnderecoBorne { get; init; }
        public string? EnderecoLogico { get; init; }

        public class Conexao
        {
            public Guid? Id { get; init; }
            public string? Tipo { get; init; }
            public string? Descricao { get; init; }
        }

        public class ModbusResponse
        {
            public int Indice { get; init; }
            public int Endereco { get; init; }
        }
    }
}
