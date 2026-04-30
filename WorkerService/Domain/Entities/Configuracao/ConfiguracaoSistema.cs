using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkerService.Domain.Entities;

public class ConfiguracaoSistema
{
    protected ConfiguracaoSistema() { }

    public ConfiguracaoSistema(string chave, string valor)
    {
        Chave = chave;
        Valor = valor;
    }

    public string Chave { get; private set; } = null!;
    public string Valor { get; private set; } = null!;

    public void Atualizar(string valor)
    {
        Valor = valor;
    }
}
