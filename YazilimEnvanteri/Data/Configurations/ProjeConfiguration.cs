using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YazilimEnvanteri.Models.Entities;

namespace YazilimEnvanteri.Data.Configurations
{
    public class ProjeConfiguration : IEntityTypeConfiguration<ProjeEntity>
    {
        public void Configure(EntityTypeBuilder<ProjeEntity> builder)
        {
            builder.ToTable("Proje");

            builder.ConfigureBaseEntity();

            builder.Property(p => p.ProjeKodu)
                .IsRequired();

            builder.HasIndex(p => p.ProjeKodu)
                .IsUnique();

            builder.Property(p => p.ProjeAdi)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(p => p.ProjeHizmetAlani)
                .IsRequired()
                .HasMaxLength(100)
                .HasComment("Projenin hizmet alanını belirtir. Örneğin: Yazılım, Altyapı, Uygulama vb.");

            builder.Property(p => p.ProjeAciklamasi)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(p => p.ProjeAktifMi)
                .IsRequired();

            builder.Property(p => p.Sunucu)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.websiteUrl)
                .IsRequired()
                .HasMaxLength(300);

            builder.Property(p => p.ProjeDurum)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(p => p.ProjeKritiklik)
                .IsRequired()
                .HasConversion<int>();

            builder.HasOne<BirimEntity>()
                .WithMany()
                .HasForeignKey(p => p.BirimId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<TeknolojiEntity>()
                .WithMany()
                .HasForeignKey(p => p.TeknolojiId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<YazilimUzmaniEntity>()
                .WithMany()
                .HasForeignKey(p => p.YazilimUzmaniId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
