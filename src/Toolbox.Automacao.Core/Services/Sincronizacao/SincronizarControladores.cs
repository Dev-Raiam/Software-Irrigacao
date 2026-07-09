using LiteDB;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using Toolbox.Automacao.Core.Data;
using Toolbox.Automacao.Core.Models;

namespace Toolbox.Automacao.Core.Services;

public interface ISincronizarControladores
{
    Task ExecutarAsync(Guid PainelId, CancellationToken cancellationToken);
}

internal sealed class SincronizarControladores : ISincronizarControladores
{
    private readonly IServicoAutomacao _servicoAutomacao;
    private readonly ILiteDatabase _database;
    private readonly ILogger<SincronizarControladores> _logger;
    private readonly IGerenciadorConfiguracao _gerenciadorConfiguracao;

    public SincronizarControladores(
        IServicoAutomacao servicoAutomacao,
        ILiteDatabase database,
        ILogger<SincronizarControladores> logger,
        IGerenciadorConfiguracao gerenciadorConfiguracao
    )
    {
        _servicoAutomacao = servicoAutomacao;
        _database = database;
        _logger = logger;
        _gerenciadorConfiguracao = gerenciadorConfiguracao;
    }

    public async Task ExecutarAsync(Guid painelId, CancellationToken cancellationToken)
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
