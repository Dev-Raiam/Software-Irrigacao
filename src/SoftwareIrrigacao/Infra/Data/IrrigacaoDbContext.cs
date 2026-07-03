using Microsoft.EntityFrameworkCore;
using Toolbox.Automacao.Core.Data;

namespace SoftwareIrrigacao.Infra.Data;

public class IrrigacaoDbContext : AutomacaoDbContext
{
    public IrrigacaoDbContext(DbContextOptions<IrrigacaoDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //modelBuilder.ApplyConfigurationsFromAssembly(typeof(IrrigacaoDbContext).Assembly);
    }
}
