using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace WorkerService.Infrastructure.Data.Mappers;

public class Interface : IEntityTypeConfiguration<Domain.Entities.Interface>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Interface> builder)
    {
        builder.ToTable("interfaces");
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => e.Id).IsUnique();
        builder.HasIndex(e => e.ModuloConectadoId).IsUnique();

        builder
            .HasOne(i => i.Modulo)
            .WithMany(m => m.Interfaces)
            .HasForeignKey(i => i.ModuloId)
            .OnDelete(DeleteBehavior.Cascade);
        builder
            .HasOne(i => i.DispositivoConectado)
            .WithMany(d => d.Interfaces)
            .HasForeignKey(i => i.DispositivoConectadoId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
