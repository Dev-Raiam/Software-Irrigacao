using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IrrigacaoInteligente.Infrastructure.Data.Mappers;

public class Controlador : IEntityTypeConfiguration<Domain.Entities.Controlador>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Controlador> builder)
    {
        builder.ToTable("controladores");
        builder.HasKey(c => c.Id);
    }
}
