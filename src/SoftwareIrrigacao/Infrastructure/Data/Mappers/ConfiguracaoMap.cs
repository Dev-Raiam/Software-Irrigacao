using SoftwareIrrigacao.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SoftwareIrrigacao.Infrastructure.Data.Mappers;

public class ConfiguracaoMap : IEntityTypeConfiguration<Configuracao>
{
    public void Configure(EntityTypeBuilder<Configuracao> builder)
    {
        builder.ToTable("configuracoes");
        builder.HasKey(e => e.Chave);
        builder.HasIndex(e => e.Chave).IsUnique();
    }
}
