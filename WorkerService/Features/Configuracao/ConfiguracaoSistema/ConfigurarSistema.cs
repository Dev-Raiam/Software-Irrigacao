using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WorkerService.Features.Configuracao.GerenciamentoCredenciais;

namespace WorkerService.Features.Configuracao.ConfiguracaoSistema;

public class ConfigurarSistema(GerenciadorCredenciais gerenciadorCredenciais)
{
    public async Task<bool> Executar(
        Guid contaId,
        string topicoConfiguracao,
        string chave,
        string segredo,
        Guid contextoId,
        CancellationToken cancellationToken
    )
    {
        var topicoConfiguracaoSalvo = await gerenciadorCredenciais.DefinirTopicoConfiguracao(
            topicoConfiguracao,
            cancellationToken
        );
        var contaIdSalva = await gerenciadorCredenciais.DefinirContaId(contaId, cancellationToken);
        var integracaoSalva = await gerenciadorCredenciais.DefinirCredencialIntegracao(
            chave,
            segredo,
            contextoId,
            cancellationToken
        );
        if (!contaIdSalva || !integracaoSalva || !topicoConfiguracaoSalvo)
        {
            return false;
        }
        return true;
    }
}
