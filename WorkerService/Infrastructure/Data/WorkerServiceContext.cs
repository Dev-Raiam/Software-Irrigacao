using Microsoft.EntityFrameworkCore;
using WorkerService.Domain.Entities;

namespace WorkerService.Infrastructure.Data;

public class WorkerServiceContext : DbContext
{
    public DbSet<Painel> Paineis { get; set; } = null!;
    public DbSet<Modulo> Modulos { get; set; } = null!;
    public DbSet<Dispositivo> Dispositivos { get; set; } = null!;
    public DbSet<Porta> Portas { get; set; } = null!;
    public DbSet<Interface> Interfaces { get; set; } = null!;
    public DbSet<ConfiguracaoSistema> ConfiguracoesSistema { get; set; } = null!;

    public WorkerServiceContext(DbContextOptions<WorkerServiceContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WorkerServiceContext).Assembly);
    }
}
