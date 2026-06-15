using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IrrigacaoInteligente.Core.DataBase.Mappers;

public class ConfiguracaoMap : IEntityTypeConfiguration<Entity.Configuracao>
{
    public void Configure(EntityTypeBuilder<Entity.Configuracao> builder)
    {
        builder.ToTable("configuracoes");
        builder.HasKey(e => e.Chave);
        builder.HasIndex(e => e.Chave).IsUnique();
    }
}
