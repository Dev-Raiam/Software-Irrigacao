using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Toolbox.Automacao.Sincronizacao.Core.Entities;

namespace Toolbox.Automacao.Sincronizacao.Infrastructure.Data.Mappers;

internal class ControladorMap : IEntityTypeConfiguration<ControladorConfiguracao>
{
    public void Configure(EntityTypeBuilder<ControladorConfiguracao> builder)
    {
        builder.ToTable("controladores_configuracao");
        builder.HasKey(c => c.Id);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };

        builder
            .Property(cg => cg.Controlador)
            .HasConversion(
                v => JsonSerializer.Serialize(v, options),
                v => JsonSerializer.Deserialize<Controlador>(v, options)!
            );
    }
}
