using Microsoft.EntityFrameworkCore;

namespace Toolbox.Automacao.Core.Data
{
    public abstract class AutomacaoDbContext : DbContext
    {
        protected AutomacaoDbContext(DbContextOptions options)
            : base(options) { }

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    modelBuilder.ApplyConfigurationsFromAssembly(typeof(AutomacaoDbContext).Assembly);
        //}
    }
}
