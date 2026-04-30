using System.Runtime.CompilerServices;
using Azure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Toolbox.Core.Api.Data;
using Toolbox.Core.Mediator;
using Toolbox.Core.Messages;
using WorkerService.Configurations;
using WorkerService.Features.Configuracao.Credenciais;
using WorkerService.Features.Shared.Abstractions;
using WorkerService.Infrastructure.Data;
using WorkerService.State;

namespace WorkerService.Features.Configuracao.ConfiguracaoSistema;

public class ConfigurarSistemaHandler(
    GerenciadorCredenciais gerenciadorCredenciais,
    IUnitOfWork<WorkerServiceContext> uow,
    WorkerServiceContext context,
    CredenciaisAplicacao credenciaisAplicacao
)
    : CommandHandler(uow),
        ICommandHandler<AdicionarCredenciais>,
        ICommandHandler<AtualizarCredenciais>
{
    public async Task<ResponseResult> Handle(
        AdicionarCredenciais request,
        CancellationToken cancellationToken
    )
    {
        if (
            !credenciaisAplicacao.Invalida
            || await gerenciadorCredenciais.VerificarCredenciaisExistentes(cancellationToken)
        )
            return Conflict("Configurações já Carregadas");

        await gerenciadorCredenciais.AdicionarPainelId(request.PainelId, cancellationToken);
        await gerenciadorCredenciais.AdicionarContaId(request.ContaId, cancellationToken);
        await gerenciadorCredenciais.AdicionarIntegracao(
            request.Integracao.Chave,
            request.Integracao.Segredo,
            request.Integracao.ContextoId,
            cancellationToken
        );

        return Ok("Configurações Salvas com sucesso");
    }

    public async Task<ResponseResult> Handle(
        AtualizarCredenciais request,
        CancellationToken cancellationToken = default
    )
    {
        var contaAtualizada = await gerenciadorCredenciais.AtualizarConta(
            request.ContaId,
            cancellationToken
        );

        var painelAtualizado = await gerenciadorCredenciais.AtualizarPainel(
            request.PainelId,
            cancellationToken
        );

        if (!contaAtualizada || !painelAtualizado)
            return NotFound();

        credenciaisAplicacao.AtualizarConta(request.ContaId);
        credenciaisAplicacao.AtualizarPainel(request.PainelId);

        var interfaces = await context.Interfaces.ToListAsync(cancellationToken);
        var portas = await context.Portas.ToListAsync(cancellationToken);
        var dispositivos = await context.Dispositivos.ToListAsync(cancellationToken);
        var modulos = await context.Modulos.ToListAsync(cancellationToken);
        var paineis = await context.Paineis.ToListAsync(cancellationToken);

        context.RemoveRange(interfaces);
        context.RemoveRange(portas);
        context.RemoveRange(dispositivos);
        context.RemoveRange(modulos);
        context.RemoveRange(paineis);

        await context.SaveChangesAsync(cancellationToken);

        return Ok("Credenciais atualizadas com sucesso");
    }
}
