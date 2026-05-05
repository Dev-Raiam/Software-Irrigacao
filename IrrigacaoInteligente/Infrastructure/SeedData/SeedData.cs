using System.Threading.Tasks;
using IrrigacaoInteligente.Features.Configuracao.Credenciais;
using IrrigacaoInteligente.Features.Shared.Abstractions;
using IrrigacaoInteligente.Infrastructure.Data;
using IrrigacaoInteligente.State;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto.Modes;

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

        var paineis = await context.Paineis.AsNoTracking().ToListAsync();
        var modulos = await context.Modulos.AsNoTracking().ToListAsync();
        var portas = await context.Portas.AsNoTracking().ToListAsync();
        var interfaces = await context.Interfaces.AsNoTracking().ToListAsync();
        var dispositivos = await context.Dispositivos.AsNoTracking().ToListAsync();

        armazenamentoAutomacao.Paineis.AddRange(paineis);
        armazenamentoAutomacao.Modulos.AddRange(modulos);
        armazenamentoAutomacao.Portas.AddRange(portas);
        armazenamentoAutomacao.Interfaces.AddRange(interfaces);
        armazenamentoAutomacao.Dispositivos.AddRange(dispositivos);

        await gerenciadorCredenciais.ObterContaId(CancellationToken.None);
        await gerenciadorCredenciais.ObterPainelId(CancellationToken.None);
        await gerenciadorCredenciais.ObterCredencialIntegracao(CancellationToken.None);
    }
}
