using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IrrigacaoInteligente.Domain.Enums;

namespace IrrigacaoInteligente.Domain.Entities;

public class Modulo
{
    protected Modulo() { }

    public Modulo(
        Guid id,
        Guid painelId,
        bool arquivado,
        bool master,
        bool controlador,
        ModuloEstagio estagio,
        ModuloMarca marca,
        ModuloModelo modelo,
        string? descricao
    )
    {
        Id = id;
        PainelId = painelId;
        Arquivado = arquivado;
        Master = master;
        Controlador = controlador;
        Estagio = estagio;
        Marca = marca;
        Modelo = modelo;
        Descricao = descricao;
    }

    public Guid Id { get; private set; }
    public Guid PainelId { get; private set; }
    public Painel Painel { get; private set; } = null!;
    public bool Arquivado { get; private set; }
    public bool Master { get; private set; }
    public bool Controlador { get; private set; }
    public ModuloEstagio Estagio { get; private set; }
    public ModuloMarca Marca { get; private set; }
    public ModuloModelo Modelo { get; private set; }
    public string? Descricao { get; private set; }

    public ICollection<Porta> Portas { get; private set; } = [];
    public ICollection<Interface> Interfaces { get; private set; } = [];

    public Modulo Atualizar(
        bool arquivado,
        bool master,
        bool controlador,
        ModuloEstagio estagio,
        ModuloMarca marca,
        ModuloModelo modelo,
        string? descricao
    )
    {
        Arquivado = arquivado;
        Master = master;
        Controlador = controlador;
        Estagio = estagio;
        Marca = marca;
        Modelo = modelo;
        Descricao = descricao;
        return this;
    }
}
