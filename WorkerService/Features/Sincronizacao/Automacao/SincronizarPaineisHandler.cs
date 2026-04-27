using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Toolbox.Core.Api.Data;
using Toolbox.Core.Mediator;
using Toolbox.Core.Messages;
using WorkerService.Configurations;
using WorkerService.Features.Shared.Extensions;
using WorkerService.Infrastructure.Data;

namespace WorkerService.Features.Sincronizacao.Automacao;

public class SincronizarPaineisCommand : Command { }

public class SincronizarPainelHandler : CommandHandler, ICommandHandler<SincronizarPaineisCommand>
{
    private readonly IAutomacaoApi _automacaoApi;
    private readonly ContaConfiguracao _contaConfiguracao;
    private readonly WorkerServiceContext _context;
    private readonly ILogger<SincronizarPainelHandler> _logger;

    public SincronizarPainelHandler(
        IAutomacaoApi automacaoApi,
        ContaConfiguracao contaConfiguracao,
        IUnitOfWork<WorkerServiceContext> uow,
        ILogger<SincronizarPainelHandler> logger
    )
        : base(uow)
    {
        _automacaoApi = automacaoApi;
        _contaConfiguracao = contaConfiguracao;
        _context = uow.Context;
        _logger = logger;
    }

    public async Task<ResponseResult> Handle(
        SincronizarPaineisCommand request,
        CancellationToken cancellationToken
    )
    {
        var paineisResponse = await _automacaoApi.ObterPaineisAsync(
            _contaConfiguracao.ContaId,
            cancellationToken
        );
        if (paineisResponse is not null)
        {
            var paineis = paineisResponse.Select(p => p.ToEntity()).ToList();

            foreach (var painel in paineis)
            {
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

            try
            {
                var salvar = await _context.SaveChangesAsync(cancellationToken);
                if (salvar > 0)
                {
                    _logger.LogInformation("Paineis sincronizados");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao salvar paineis");
            }
        }
        return Ok<ResponseResult>();
    }
}
