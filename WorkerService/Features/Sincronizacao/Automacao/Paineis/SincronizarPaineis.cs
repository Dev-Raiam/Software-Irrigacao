using Microsoft.EntityFrameworkCore;
using WorkerService.Application.Extensions;
using WorkerService.Configurations;
using WorkerService.Infrastructure.Data;
using WorkerService.Infrastructure.Interfaces;

namespace WorkerService.Features.Sincronizacao.Automacao.Paineis;

public class SincronizarPainel(
    IAutomacaoApi _automacaoApi,
    ContaConfiguracao _contaConfiguracao,
    WorkerServiceContext _context
)
{
    public async Task<List<Guid>> ExecutarSincronizacao(CancellationToken cancellationToken)
    {
        var painelIds = new List<Guid>();
        var paineisResponse = await _automacaoApi.ObterPaineisAsync(
            _contaConfiguracao.ContaId,
            cancellationToken
        );

        if (paineisResponse is not null)
        {
            var paineis = paineisResponse.Select(p => p.ToEntity()).ToList();

            foreach (var painel in paineis)
            {
                painelIds.Add(painel.Id);
                var painelExistente = await _context
                    .Paineis.Include(p => p.Modulos)
                    .FirstOrDefaultAsync(p => p.Id == painel.Id, cancellationToken);
                if (painelExistente == null)
                {
                    await _context.Paineis.AddAsync(painel, cancellationToken);
                }
                else
                {
                    painelExistente.Atualizar(
                        painel.Arquivado,
                        painel.Primario,
                        painel.Descricao,
                        painel.Referencia,
                        painel.Modulos
                    );
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
        return painelIds;
    }
}
