using YazilimEnvanteri.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace YazilimEnvanteri.Data.Configurations
{
    public class YazilimUzmaniConfiguraiton : IEntityTypeConfiguration<ProjeEntity>
    {
        public void Configure(EntityTypeBuilder<ProjeEntity> builder)
        {
            builder.ToTable("Proje", "Proje");

            builder.Property(p => p.ProjeKodu)
                .HasMaxLength(3)
                .IsRequired();

            builder.Property(p => p.ProjeAdi)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.ProjeHizmetAlani)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.ProjeAciklamasi)
                .HasMaxLength(500);

            builder.Property(p => p.ProjeAktifMi)
                .IsRequired();
 
            builder.Property(p => p.Sunucu)
                .HasMaxLength(100);

        }
    }
}
