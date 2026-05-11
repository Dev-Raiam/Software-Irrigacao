using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IrrigacaoInteligente.Infrastructure.Data.Mappers;

public class Telemetria : IEntityTypeConfiguration<Domain.Entities.Telemetria>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Telemetria> builder)
    {
        builder.ToTable("telemetrias");
        builder.HasKey(t => t.Id);
    }
}
