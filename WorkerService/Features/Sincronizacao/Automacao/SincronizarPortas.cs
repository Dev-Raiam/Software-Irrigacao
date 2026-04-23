using Microsoft.EntityFrameworkCore;
using WorkerService.Features.Shared.Extensions;
using WorkerService.Infrastructure.Data;

namespace WorkerService.Features.Sincronizacao.Automacao;

public class SincronizarPortas(IAutomacaoApi _automacaoApi, WorkerServiceContext _context)
{
    public async Task<bool> ExecutarSincronizacaoControlador(
        List<Domain.Entities.Modulo> controladores,
        CancellationToken cancellationToken
    )
    {
        var sucesso = false;
        foreach (var controlador in controladores)
        {
            var portasResponse = await _automacaoApi.ObterPortasPorControladorAsync(
                controlador.PainelId,
                controlador.Id,
                cancellationToken
            );

            if (portasResponse is not null)
            {
                foreach (var portaResponse in portasResponse)
                {
                    var porta = portaResponse.ToEntity(controlador.Id);

                    var portaExistente = await _context.Portas.FirstOrDefaultAsync(
                        p => p.Id == porta.Id,
                        cancellationToken
                    );

                    if (portaExistente is null)
                    {
                        await _context.Portas.AddAsync(porta, cancellationToken);
                    }
                    else
                    {
                        portaExistente.Atualizar(
                            porta.DispositivoConectadoId,
                            porta.Tipo,
                            porta.Sinal,
                            porta.Status,
                            porta.Nome,
                            porta.EnderecoBorne,
                            porta.EnderecoLogico
                        );
                    }
                }
                await _context.SaveChangesAsync(cancellationToken);
                sucesso = true;
            }
        }
        return sucesso;
    }

    public async Task<bool> ExecutarSincronizacaoModulo(
        List<Domain.Entities.Modulo> modulos,
        CancellationToken cancellationToken
    )
    {
        var sucesso = false;
        foreach (var modulo in modulos)
        {
            var portasResponse = await _automacaoApi.ObterPortasPorModuloAsync(
                modulo.PainelId,
                modulo.Id,
                cancellationToken
            );

            if (portasResponse is not null)
            {
                foreach (var portaResponse in portasResponse)
                {
                    var porta = portaResponse.ToEntity(modulo.Id);

                    var portaExistente = await _context.Portas.FirstOrDefaultAsync(
                        p => p.Id == porta.Id,
                        cancellationToken
                    );

                    if (portaExistente is null)
                    {
                        await _context.Portas.AddAsync(porta, cancellationToken);
                    }
                    else
                    {
                        portaExistente.Atualizar(
                            porta.DispositivoConectadoId,
                            porta.Tipo,
                            porta.Sinal,
                            porta.Status,
                            porta.Nome,
                            porta.EnderecoBorne,
                            porta.EnderecoLogico
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
