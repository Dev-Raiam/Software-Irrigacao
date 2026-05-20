using IrrigacaoInteligente.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Toolbox.Core.Api.Data;
using Toolbox.Core.Models;

namespace IrrigacaoInteligente.Infrastructure.Data;

public class User : IUser
{
    public string? Name { get; set; }
    public string? Email { get; set; }

    public long? Id => null;

    public string? Role => null;

    public long? TenantId => null;

    public Guid? ClientId => null;

    public Guid? FeatureId => null;

    public Guid? CollaboratorId => null;

    public bool Authenticated => true;
}

public class IrrigacaoInteligenteContext : DbContext, IDataContext
{
    public DbSet<Configuracao> Configuracoes { get; set; } = null!;
    public DbSet<Controlador> Controladores { get; set; } = null!;

    // public DbSet<Telemetria> Telemetrias { get; set; } = null!;

    public IUser User { get; }

    public IrrigacaoInteligenteContext(DbContextOptions<IrrigacaoInteligenteContext> options)
        : base(options)
    {
        User = new User();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IrrigacaoInteligenteContext).Assembly);
    }
}
