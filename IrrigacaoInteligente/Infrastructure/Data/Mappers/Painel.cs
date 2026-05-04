using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IrrigacaoInteligente.Infrastructure.Data.Mappers;

public class Painel : IEntityTypeConfiguration<Domain.Entities.Painel>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Painel> builder)
    {
        builder.ToTable("paineis");
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => e.Id).IsUnique();

        builder
            .HasMany(p => p.Modulos)
            .WithOne(m => m.Painel)
            .HasForeignKey(m => m.PainelId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
