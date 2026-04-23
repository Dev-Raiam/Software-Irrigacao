using Microsoft.EntityFrameworkCore;
using WorkerService.Features.Shared.Extensions;
using WorkerService.Infrastructure.Data;

namespace WorkerService.Features.Sincronizacao.Automacao;

public class SincronizarInterfaces(IAutomacaoApi _automacaoApi, WorkerServiceContext _context)
{
    public async Task<bool> ExecutarSincronizacaoControlador(
        List<Domain.Entities.Modulo> controladores,
        CancellationToken cancellationToken
    )
    {
        var sucesso = false;
        foreach (var controlador in controladores)
        {
            var interfaceResponses = await _automacaoApi.ObterInterfacesPorControladorAsync(
                controlador.PainelId,
                controlador.Id,
                cancellationToken
            );

            if (interfaceResponses is not null)
            {
                foreach (var interfaceResponse in interfaceResponses)
                {
                    var interfaceEntity = interfaceResponse.ToEntity(controlador.Id);

                    var interfaceExistente = await _context.Interfaces.FirstOrDefaultAsync(
                        i => i.Id == interfaceEntity.Id,
                        cancellationToken
                    );

                    if (interfaceExistente is null)
                    {
                        await _context.Interfaces.AddAsync(interfaceEntity, cancellationToken);
                    }
                    else
                    {
                        interfaceExistente.Atualizar(
                            controlador.Id,
                            interfaceEntity.ModuloConectadoId,
                            interfaceEntity.Tipo,
                            interfaceEntity.Status,
                            interfaceEntity.Marca,
                            interfaceEntity.Modelo,
                            interfaceEntity.Categoria,
                            interfaceEntity.IndiceModbus,
                            interfaceEntity.EnderecoModbus,
                            interfaceEntity.Porta,
                            interfaceEntity.EnderecoBorne,
                            interfaceEntity.EnderecoLogico
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
            var interfaceResponses = await _automacaoApi.ObterInterfacesPorModuloAsync(
                modulo.PainelId,
                modulo.Id,
                cancellationToken
            );

            if (interfaceResponses is not null)
            {
                foreach (var interfaceResponse in interfaceResponses)
                {
                    var interfaceEntity = interfaceResponse.ToEntity(modulo.Id);

                    var interfaceExistente = await _context.Interfaces.FirstOrDefaultAsync(
                        i => i.Id == interfaceEntity.Id,
                        cancellationToken
                    );

                    if (interfaceExistente is null)
                    {
                        await _context.Interfaces.AddAsync(interfaceEntity, cancellationToken);
                    }
                    else
                    {
                        interfaceExistente.Atualizar(
                            modulo.Id,
                            interfaceEntity.ModuloConectadoId,
                            interfaceEntity.Tipo,
                            interfaceEntity.Status,
                            interfaceEntity.Marca,
                            interfaceEntity.Modelo,
                            interfaceEntity.Categoria,
                            interfaceEntity.IndiceModbus,
                            interfaceEntity.EnderecoModbus,
                            interfaceEntity.Porta,
                            interfaceEntity.EnderecoBorne,
                            interfaceEntity.EnderecoLogico
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
