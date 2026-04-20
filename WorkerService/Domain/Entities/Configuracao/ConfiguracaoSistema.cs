using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkerService.Domain.Entities;

[Table("ConfiguracoesSistema")]
public class ConfiguracaoSistema
{
    public string Chave { get; private set; } = null!;
    public string Valor { get; private set; } = null!;

    public ConfiguracaoSistema() { }

    public ConfiguracaoSistema(string chave, string valor)
    {
        Chave = chave;
        Valor = valor;
    }

    public void Atualizar(string valor)
    {
        Valor = valor;
    }
}
