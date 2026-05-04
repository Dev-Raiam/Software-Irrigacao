using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IrrigacaoInteligente.Infrastructure.Data.Mappers;

public class Dispositivo : IEntityTypeConfiguration<Domain.Entities.Dispositivo>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Dispositivo> builder)
    {
        builder.ToTable("dispositivos");
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => e.Id).IsUnique();
        builder.HasIndex(e => e.PainelId);

        builder
            .HasOne(d => d.Painel)
            .WithMany()
            .HasForeignKey(d => d.PainelId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.OwnsOne(
            d => d.Parametros,
            p =>
            {
                p.ToJson();
                p.Property(x => x.UnidadeMedida).HasConversion<string>();
            }
        );
    }
}
