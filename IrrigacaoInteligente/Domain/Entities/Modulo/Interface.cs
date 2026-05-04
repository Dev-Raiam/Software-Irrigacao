using IrrigacaoInteligente.Domain.Enums;

namespace IrrigacaoInteligente.Domain.Entities;

public class Interface
{
    protected Interface() { }

    public Interface(
        Guid id,
        Guid moduloId,
        Guid? moduloConectadoId,
        InterfaceTipo tipo,
        PortaStatus status,
        ModuloMarca marca,
        ModuloModelo modelo,
        ModuloCategoria categoria,
        int? indiceModbus,
        int? enderecoModbus,
        string porta,
        string? enderecoBorne,
        string? enderecoLogico,
        Guid? dispositivoConectadoId = null
    )
    {
        Id = id;
        ModuloId = moduloId;
        ModuloConectadoId = moduloConectadoId;
        DispositivoConectadoId = dispositivoConectadoId;
        Tipo = tipo;
        Status = status;
        Marca = marca;
        Modelo = modelo;
        Categoria = categoria;
        IndiceModbus = indiceModbus;
        EnderecoModbus = enderecoModbus;
        Porta = porta;
        EnderecoBorne = enderecoBorne;
        EnderecoLogico = enderecoLogico;
    }

    public Guid Id { get; private set; }
    public Guid ModuloId { get; private set; }
    public Guid? ModuloConectadoId { get; private set; }
    public Modulo Modulo { get; private set; } = null!;
    public Guid? DispositivoConectadoId { get; private set; }
    public Dispositivo? DispositivoConectado { get; private set; }
    public InterfaceTipo Tipo { get; private set; }
    public PortaStatus Status { get; private set; }
    public ModuloMarca Marca { get; private set; }
    public ModuloModelo Modelo { get; private set; }
    public ModuloCategoria Categoria { get; private set; }
    public int? IndiceModbus { get; private set; }
    public int? EnderecoModbus { get; private set; }
    public string Porta { get; private set; } = null!;
    public string? EnderecoBorne { get; private set; }
    public string? EnderecoLogico { get; private set; }

    public Interface Atualizar(
        Guid moduloId,
        Guid? moduloConectadoId,
        InterfaceTipo tipo,
        PortaStatus status,
        ModuloMarca marca,
        ModuloModelo modelo,
        ModuloCategoria categoria,
        int? indiceModbus,
        int? enderecoModbus,
        string porta,
        string? enderecoBorne,
        string? enderecoLogico,
        Guid? dispositivoConectadoId = null
    )
    {
        ModuloId = moduloId;
        ModuloConectadoId = moduloConectadoId;
        DispositivoConectadoId = dispositivoConectadoId;
        Tipo = tipo;
        Status = status;
        Marca = marca;
        Modelo = modelo;
        Categoria = categoria;
        IndiceModbus = indiceModbus;
        EnderecoModbus = enderecoModbus;
        Porta = porta;
        EnderecoBorne = enderecoBorne;
        EnderecoLogico = enderecoLogico;

        return this;
    }
}
