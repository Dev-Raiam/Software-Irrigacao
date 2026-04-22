using System.Reflection.Metadata.Ecma335;
using Microsoft.EntityFrameworkCore;
using WorkerService.Features.Automacao.Sincronizacao.Dispositivos;
using WorkerService.Features.Automacao.Sincronizacao.InterfacesComunicacao;
using WorkerService.Features.Automacao.Sincronizacao.Paineis;
using WorkerService.Features.Automacao.Sincronizacao.Portas;
using WorkerService.Infrastructure.Data;

namespace WorkerService.Features.Automacao.Sincronizacao;

public class SincronizarAutomacao(
    SincronizarPainel _paineis,
    SincronizarDispositivos _dispositivos,
    SincronizarPortas _portas,
    SincronizarInterfaces _interfaces,
    WorkerServiceContext _context
)
{
    public async Task<bool> Executar(CancellationToken cancellationToken)
    {
        var painelIds = await _paineis.ExecutarSincronizacao(cancellationToken);
        if (painelIds.Count == 0)
            return false;

        if (!await _dispositivos.ExecutarSincronizacao(painelIds, cancellationToken))
            return false;

        var modulos = await _context.Modulos.AsNoTracking().ToListAsync(cancellationToken);
        var controladores = modulos.Where(m => m.Controlador).ToList();
        var modulosSemControlador = modulos.Where(m => !m.Controlador).ToList();

        if (!await _portas.ExecutarSincronizacaoControlador(controladores, cancellationToken))
            return false;

        if (!await _portas.ExecutarSincronizacaoModulo(modulosSemControlador, cancellationToken))
            return false;

        if (!await _interfaces.ExecutarSincronizacaoControlador(controladores, cancellationToken))
            return false;

        if (
            !await _interfaces.ExecutarSincronizacaoModulo(modulosSemControlador, cancellationToken)
        )
            return false;

        return true;
    }
}
