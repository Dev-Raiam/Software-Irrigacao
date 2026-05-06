using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IrrigacaoInteligente.Infrastructure.Data.Mappers;

public class Configuracao : IEntityTypeConfiguration<Domain.Entities.Configuracao>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Configuracao> builder)
    {
        builder.ToTable("configuracoes");
        builder.HasKey(e => e.Chave);
        builder.HasIndex(e => e.Chave).IsUnique();
    }
}
