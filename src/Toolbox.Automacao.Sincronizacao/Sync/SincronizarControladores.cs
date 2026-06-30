using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Toolbox.Automacao.Sincronizacao.Core.Abstractions;
using Toolbox.Automacao.Sincronizacao.Core.Entities;
using Toolbox.Automacao.Sincronizacao.Infrastructure.Data;

namespace Toolbox.Automacao.Sincronizacao.Sync;

internal class SincronizarControladores : ISincronizarControladores
{
    private readonly IApiAutomacao _apiAutomacao;
    private readonly ILogger<SincronizarControladores> _logger;
    private readonly SincronizacaoDbContext _context;

    public SincronizarControladores(
        IApiAutomacao apiAutomacao,
        ILogger<SincronizarControladores> logger,
        SincronizacaoDbContext context
    )
    {
        _apiAutomacao = apiAutomacao;
        _logger = logger;
        _context = context;
    }

    public async Task ExecutarAsync(Guid painelId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Sincronizando dados");

        var result = await _apiAutomacao.ObterControladoresPorPainelAsync(
            painelId,
            cancellationToken
        );

        if (result.Sucesso && result.Dado != null)
        {
            await _context.ControladoresConfiguracao.ExecuteDeleteAsync(cancellationToken);

            foreach (var controlador in result.Dado)
            {
                await _context.ControladoresConfiguracao.AddAsync(
                    new ControladorConfiguracao(controlador),
                    cancellationToken
                );

                await _context.SaveChangesAsync();
            }

            _logger.LogInformation("Dados Sincronizados com sucesso");
        }
        else
        {
            _logger.LogError("Falha ao obter controladores: {Error} ", result.Error);

            if (result.Exception != null)
            {
                _logger.LogError("Exception: {Exception}", result.Exception.Message);
            }
        }
    }
}
