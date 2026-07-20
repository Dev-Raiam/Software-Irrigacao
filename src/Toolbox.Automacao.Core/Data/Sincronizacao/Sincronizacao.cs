using LiteDB;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using Toolbox.Automacao.Core.Models;
using Toolbox.Automacao.Core.Services;

namespace Toolbox.Automacao.Core.Data;

public interface ISincronizacao
{
    Task SincronizarAutomacao(Guid PainelId, CancellationToken cancellationToken);
}

internal sealed class Sincronizacao : ISincronizacao
{
    private readonly IServicoAutomacao _servicoAutomacao;
    private readonly ILiteDatabase _database;
    private readonly ILogger<Sincronizacao> _logger;
    private readonly IGerenciadorConfiguracao _gerenciadorConfiguracao;

    public Sincronizacao(
        IServicoAutomacao servicoAutomacao,
        ILiteDatabase database,
        ILogger<Sincronizacao> logger,
        IGerenciadorConfiguracao gerenciadorConfiguracao
    )
    {
        _servicoAutomacao = servicoAutomacao;
        _database = database;
        _logger = logger;
        _gerenciadorConfiguracao = gerenciadorConfiguracao;
    }

    public async Task SincronizarAutomacao(Guid painelId, CancellationToken cancellationToken)
    {
        using (LogContext.PushProperty("PainelId", painelId))
        {
            _logger.LogInformation("Sincronização Iniciada...");

            if (!_gerenciadorConfiguracao.ExisteCredenciaisIntegracao())
            {
                _logger.LogWarning("Sincronização cancelada: credenciais de integracao não configuradas");
                return;
            }

            var result = await _servicoAutomacao.ObterControladoresPorPainelAsync(
                painelId,
                cancellationToken
            );

            if (result.Sucesso && result.Dado != null)
            {
                foreach (var controlador in result.Dado)
                {
                    _database
                        .GetCollection<ControladorConfiguracao>(Tabela.Controladores)
                        .Upsert(new ControladorConfiguracao(controlador));
                }

                var quantidadeControladores = result.Dado.Count;
                _logger.LogInformation("Sincronização concluída: {QuantidadeControladores} controladores", quantidadeControladores);
            }
            else
            {
                _logger.LogWarning(
                    "Falha ao obter controladores: {Error}",
                    result.Error
                );
            }
        }
    }
}
