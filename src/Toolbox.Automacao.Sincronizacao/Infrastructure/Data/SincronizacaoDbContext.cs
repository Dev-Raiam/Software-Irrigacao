using Microsoft.EntityFrameworkCore;
using Toolbox.Automacao.Sincronizacao.Core.Entities;

namespace Toolbox.Automacao.Sincronizacao.Infrastructure.Data;

public class SincronizacaoDbContext : DbContext
{
    public DbSet<ControladorConfiguracao> ControladoresConfiguracao { get; set; } = null!;

    public SincronizacaoDbContext(DbContextOptions options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SincronizacaoDbContext).Assembly);
    }
}
