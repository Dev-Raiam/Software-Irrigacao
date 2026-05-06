using IrrigacaoInteligente.Features.Credenciais;
using IrrigacaoInteligente.Infrastructure.Data;
using IrrigacaoInteligente.State;
using Microsoft.EntityFrameworkCore;

namespace IrrigacaoInteligente.Infrastructure.SeedData;

public static class SeedData
{
    public static async Task Seed(IServiceProvider serviceProvider)
    {
        using var scoped = serviceProvider.CreateScope();

        var context = scoped.ServiceProvider.GetRequiredService<IrrigacaoInteligenteContext>();

        var gerenciadorCredenciais =
            scoped.ServiceProvider.GetRequiredService<GerenciadorCredenciais>();

        var armazenamentoAutomacao =
            scoped.ServiceProvider.GetRequiredService<ArmazenamentoAutomacao>();

        var armazenamentoCredenciais =
            scoped.ServiceProvider.GetRequiredService<CredenciaisAplicacao>();

        await context.Database.MigrateAsync();

        // var paineis = await context.Paineis.AsNoTracking().ToListAsync();
        // var modulos = await context.Modulos.AsNoTracking().ToListAsync();
        // var portas = await context.Portas.AsNoTracking().ToListAsync();
        // var interfaces = await context.Interfaces.AsNoTracking().ToListAsync();
        // var dispositivos = await context.Dispositivos.AsNoTracking().ToListAsync();

        var controlador = await context.Controladores.AsNoTracking().FirstOrDefaultAsync();

        armazenamentoAutomacao.Dados = controlador?.Configuracao ?? string.Empty;
        // armazenamentoAutomacao.Paineis.AddRange(paineis);
        // armazenamentoAutomacao.Modulos.AddRange(modulos);
        // armazenamentoAutomacao.Portas.AddRange(portas);
        // armazenamentoAutomacao.Interfaces.AddRange(interfaces);
        // armazenamentoAutomacao.Dispositivos.AddRange(dispositivos);

        var contaId = await gerenciadorCredenciais.ObterContaId(CancellationToken.None);
        var painelId = await gerenciadorCredenciais.ObterPainelId(CancellationToken.None);
        var credenciaisIntegracao = await gerenciadorCredenciais.ObterCredencialIntegracao(
            CancellationToken.None
        );
        var chave = credenciaisIntegracao.chave;
        var segredo = credenciaisIntegracao.segredo;
        var contextoId = credenciaisIntegracao.contextoId;

        if (
            contaId is not null
            && painelId is not null
            && chave is not null
            && segredo is not null
            && contextoId is not null
        )
        {
            armazenamentoCredenciais.AdicionarConta(contaId.Value);
            armazenamentoCredenciais.AdicionarPainel(painelId.Value);
            armazenamentoCredenciais.AdicionarIntegracao(chave, segredo, contextoId.Value);
        }

        // armazenamentoCredenciais.PainelId = painelId;
        // armazenamentoCredenciais.CredenciaisIntegracao = credenciaisIntegracao;
    }
}
