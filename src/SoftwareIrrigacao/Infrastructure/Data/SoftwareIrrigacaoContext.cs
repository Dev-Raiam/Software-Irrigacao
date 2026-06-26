using SoftwareIrrigacao.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Toolbox.Automacao.Sincronizacao.Infrastructure.Data;

namespace SoftwareIrrigacao.Data;

public class SoftwareIrrigacaoContext : SincronizacaoDbContext
{
    public DbSet<Configuracao> Configuracoes { get; set; } = null!;

    public SoftwareIrrigacaoContext(DbContextOptions<SoftwareIrrigacaoContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SoftwareIrrigacaoContext).Assembly);
    }
}
