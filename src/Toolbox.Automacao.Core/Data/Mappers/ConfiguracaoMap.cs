using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Toolbox.Automacao.Core.Models;

namespace Toolbox.Automacao.Core.Data.Mappers;

public class ConfiguracaoMap : IEntityTypeConfiguration<Configuracao>
{
    public void Configure(EntityTypeBuilder<Configuracao> builder)
    {
        builder.ToTable("configuracoes");
        builder.HasKey(e => e.Chave);
        builder.HasIndex(e => e.Chave).IsUnique();
    }
}
