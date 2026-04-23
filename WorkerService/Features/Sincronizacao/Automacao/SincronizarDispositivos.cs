using System.Reflection.Metadata.Ecma335;
using Microsoft.EntityFrameworkCore;
using WorkerService.Features.Shared.Extensions;
using WorkerService.Infrastructure.Data;

namespace WorkerService.Features.Sincronizacao.Automacao;

public class SincronizarDispositivos(IAutomacaoApi _automacaoApi, WorkerServiceContext _context)
{
    public async Task<bool> ExecutarSincronizacao(
        List<Guid> painelIds,
        CancellationToken cancellationToken
    )
    {
        var sucesso = false;
        foreach (var painelId in painelIds)
        {
            var dispositivosResponse = await _automacaoApi.ObterDispositivosPorPainelAsync(
                painelId,
                cancellationToken
            );

            if (dispositivosResponse is not null)
            {
                foreach (var dispositivoResponse in dispositivosResponse)
                {
                    var dispositivo = dispositivoResponse.ToEntity(painelId);

                    var dispositivoExistente = await _context.Dispositivos.FirstOrDefaultAsync(
                        d => d.Id == dispositivo.Id,
                        cancellationToken
                    );

                    if (dispositivoExistente is null)
                    {
                        await _context.Dispositivos.AddAsync(dispositivo, cancellationToken);
                    }
                    else
                    {
                        dispositivoExistente.Atualizar(
                            dispositivo.Arquivado,
                            dispositivo.Tipo,
                            dispositivo.Parametros,
                            dispositivo.Descricao
                        );
                    }
                }
                await _context.SaveChangesAsync(cancellationToken);
                sucesso = true;
            }
        }
        return sucesso;
    }
}
