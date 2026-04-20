using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace WorkerService.Infrastructure.Data.Mappers;

public class ConfiguracaoSistema : IEntityTypeConfiguration<Domain.Entities.ConfiguracaoSistema>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.ConfiguracaoSistema> builder)
    {
        builder.ToTable("configuracoes_sistema");
        builder.HasKey(e => e.Chave);
        builder.HasIndex(e => e.Chave).IsUnique();
    }
}
