using IrrigacaoInteligente.Core.DataBase.Entity;
using Microsoft.EntityFrameworkCore;
using Toolbox.Automacao.Sincronizacao.Data;

namespace IrrigacaoInteligente.Core.DataBase;

public class IrrigacaoInteligenteContext : SincronizacaoDbContext
{
    public DbSet<Configuracao> Configuracoes { get; set; } = null!;

    public IrrigacaoInteligenteContext(DbContextOptions<IrrigacaoInteligenteContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IrrigacaoInteligenteContext).Assembly);
    }
}
