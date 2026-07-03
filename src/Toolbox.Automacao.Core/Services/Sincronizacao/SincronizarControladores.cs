using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Toolbox.Automacao.Core.Data;
using Toolbox.Automacao.Core.Models;

namespace Toolbox.Automacao.Core.Services;

internal sealed class SincronizarControladores : ISincronizarControladores
{
    private readonly IServicoAutomacao _servicoAutomacao;
    private readonly ILogger<SincronizarControladores> _logger;
    private readonly AutomacaoDbContext _context;

    public SincronizarControladores(
        IServicoAutomacao servicoAutomacao,
        ILogger<SincronizarControladores> logger,
        AutomacaoDbContext context
    )
    {
        _servicoAutomacao = servicoAutomacao;
        _logger = logger;
        _context = context;
    }

    public async Task ExecutarAsync(Guid painelId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Sincronizando dados");

        var result = await _servicoAutomacao.ObterControladoresPorPainelAsync(
            painelId,
            cancellationToken
        );

        if (result.Sucesso && result.Dado != null)
        {
            await _context.Set<ControladorConfiguracao>().ExecuteDeleteAsync(cancellationToken);
            // Verificar se existe se sim Update se nao Update

            foreach (var controlador in result.Dado)
            {
                await _context.Set<ControladorConfiguracao>().AddAsync(
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
