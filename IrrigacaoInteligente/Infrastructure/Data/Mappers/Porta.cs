// using Microsoft.EntityFrameworkCore;
// using Microsoft.EntityFrameworkCore.Metadata.Builders;
// using IrrigacaoInteligente.Domain.Entities;

// namespace IrrigacaoInteligente.Infrastructure.Data.Mappers;

// public class Porta : IEntityTypeConfiguration<Domain.Entities.Porta>
// {
//     public void Configure(EntityTypeBuilder<Domain.Entities.Porta> builder)
//     {
//         builder.ToTable("portas");
//         builder.HasKey(e => e.Id);
//         builder.HasIndex(e => e.Id).IsUnique();
//         builder.HasIndex(e => e.ModuloId);
//         builder.HasIndex(e => e.DispositivoConectadoId);
//         builder.HasIndex(e => e.EnderecoLogico);

//         builder
//             .HasOne(p => p.Modulo)
//             .WithMany(m => m.Portas)
//             .HasForeignKey(p => p.ModuloId)
//             .OnDelete(DeleteBehavior.Cascade);

//         builder
//             .HasOne(p => p.DispositivoConectado)
//             .WithOne(d => d.Porta)
//             .HasForeignKey<Domain.Entities.Porta>(p => p.DispositivoConectadoId)
//             .OnDelete(DeleteBehavior.SetNull);
//     }
// }
