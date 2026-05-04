using System.Runtime.CompilerServices;
using Azure;
using IrrigacaoInteligente.Configurations;
using IrrigacaoInteligente.Features.Configuracao.Credenciais;
using IrrigacaoInteligente.Features.Shared.Abstractions;
using IrrigacaoInteligente.Infrastructure.Data;
using IrrigacaoInteligente.State;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Toolbox.Core.Api.Data;
using Toolbox.Core.Mediator;
using Toolbox.Core.Messages;

namespace IrrigacaoInteligente.Features.Configuracao.ConfiguracaoSistema;

public class ConfigurarSistemaHandler
    : CommandHandler,
        ICommandHandler<AdicionarCredenciais>,
        ICommandHandler<AtualizarCredenciais>
{
    private readonly GerenciadorCredenciais _gerenciadorCredenciais;
    private readonly IrrigacaoInteligenteContext _context;
    private readonly CredenciaisAplicacao _credenciaisAplicacao;

    public ConfigurarSistemaHandler(
        GerenciadorCredenciais gerenciadorCredenciais,
        IUnitOfWork<IrrigacaoInteligenteContext> uow,
        IrrigacaoInteligenteContext context,
        CredenciaisAplicacao credenciaisAplicacao
    )
        : base(uow)
    {
        _gerenciadorCredenciais = gerenciadorCredenciais;
        _context = context;
        _credenciaisAplicacao = credenciaisAplicacao;
    }

    public async Task<ResponseResult> Handle(
        AdicionarCredenciais request,
        CancellationToken cancellationToken
    )
    {
        if (
            !_credenciaisAplicacao.Invalida
            || await _gerenciadorCredenciais.VerificarCredenciaisExistentes(cancellationToken)
        )
            return Conflict("Configurações já Carregadas");

        await _gerenciadorCredenciais.AdicionarPainelId(request.PainelId, cancellationToken);
        await _gerenciadorCredenciais.AdicionarContaId(request.ContaId, cancellationToken);
        await _gerenciadorCredenciais.AdicionarIntegracao(
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
        var contaAtualizada = await _gerenciadorCredenciais.AtualizarConta(
            request.ContaId,
            cancellationToken
        );

        var painelAtualizado = await _gerenciadorCredenciais.AtualizarPainel(
            request.PainelId,
            cancellationToken
        );

        if (!contaAtualizada || !painelAtualizado)
            return NotFound();

        _credenciaisAplicacao.AtualizarConta(request.ContaId);
        _credenciaisAplicacao.AtualizarPainel(request.PainelId);

        var interfaces = await _context.Interfaces.ToListAsync(cancellationToken);
        var portas = await _context.Portas.ToListAsync(cancellationToken);
        var dispositivos = await _context.Dispositivos.ToListAsync(cancellationToken);
        var modulos = await _context.Modulos.ToListAsync(cancellationToken);
        var paineis = await _context.Paineis.ToListAsync(cancellationToken);

        _context.RemoveRange(interfaces);
        _context.RemoveRange(portas);
        _context.RemoveRange(dispositivos);
        _context.RemoveRange(modulos);
        _context.RemoveRange(paineis);

        await _context.SaveChangesAsync(cancellationToken);

        return Ok("Credenciais atualizadas com sucesso");
    }
}
