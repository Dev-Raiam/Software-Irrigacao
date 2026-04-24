using Microsoft.EntityFrameworkCore;
using WorkerService.Features.Shared.Abstractions;
using WorkerService.Infrastructure.Data;

namespace WorkerService.Features.Sincronizacao.Automacao;

public class SincronizarAutomacao(
    SincronizarPainel _paineis,
    SincronizarDispositivos _dispositivos,
    SincronizarPortas _portas,
    SincronizarInterfaces _interfaces,
    WorkerServiceContext _context,
    ILogger<SincronizarAutomacao> _logger
)
{
    public async Task<bool> Executar(CancellationToken cancellationToken)
    {
        var painelIds = await _paineis.ExecutarSincronizacao(cancellationToken);
        if (painelIds.Count == 0)
            return false;

        var sucessoDispositivos = await _dispositivos.ExecutarSincronizacao(
            painelIds,
            cancellationToken
        );

        var modulos = await _context.Modulos.AsNoTracking().ToListAsync(cancellationToken);
        var controladores = modulos.Where(m => m.Controlador).ToList();
        var modulosSemControlador = modulos.Where(m => !m.Controlador).ToList();

        var sucessoPortasControlador = await _portas.ExecutarSincronizacaoControlador(
            controladores,
            cancellationToken
        );

        var sucessoPortasModulo = await _portas.ExecutarSincronizacaoModulo(
            modulosSemControlador,
            cancellationToken
        );

        var sucessoInterfacesControlador = await _interfaces.ExecutarSincronizacaoControlador(
            controladores,
            cancellationToken
        );

        var sucessoInterfacesModulo = await _interfaces.ExecutarSincronizacaoModulo(
            modulosSemControlador,
            cancellationToken
        );
        if (
            !sucessoDispositivos
            || !sucessoPortasControlador
            || !sucessoPortasModulo
            || !sucessoInterfacesControlador
            || !sucessoInterfacesModulo
        )
        {
            _logger.LogWarning(
                "Sincronização dos dados de Automação não foi executada com sucesso"
            );
            return false;
        }

        _logger.LogInformation("Sincronização dos dados de Automação executada com sucesso");
        return true;
    }
}
