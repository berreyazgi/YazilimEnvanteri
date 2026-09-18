using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YazilimEnvanteri.Models.Entities;

namespace YazilimEnvanteri.Data.Configurations
{
    public class YazilimUzmaniConfiguration : IEntityTypeConfiguration<YazilimUzmaniEntity>
    {
        public void Configure(EntityTypeBuilder<YazilimUzmaniEntity> builder)
        {
            builder.ToTable("YazilimUzmanlari");

            builder.ConfigureBaseEntity();

            builder.Property(y => y.KullanıcıAdi)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(y => y.Ad)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(y => y.Soyad)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(y => y.Gorev)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(y => y.Email)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(y => y.Telefon)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(y => y.SorumluFirma)
                .IsRequired()
                .HasMaxLength(150);

            builder.HasIndex(y => y.KullanıcıAdi);
            builder.HasIndex(y => y.Email);

            builder.HasOne<BirimEntity>()
                .WithMany()
                .HasForeignKey(y => y.Birim)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<ProjeEntity>()
                .WithMany()
                .HasForeignKey(y => y.ProjeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
