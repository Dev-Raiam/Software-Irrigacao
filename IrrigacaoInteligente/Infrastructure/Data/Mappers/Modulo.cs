using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IrrigacaoInteligente.Infrastructure.Data.Mappers;

public class Modulo : IEntityTypeConfiguration<Domain.Entities.Modulo>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Modulo> builder)
    {
        builder.ToTable("modulos");
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => e.Id).IsUnique();
        builder.HasIndex(e => e.PainelId);
    }
}
