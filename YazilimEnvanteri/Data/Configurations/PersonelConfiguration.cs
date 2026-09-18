using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YazilimEnvanteri.Models.Entities;

namespace YazilimEnvanteri.Data.Configurations
{
    public class PersonelConfiguration : IEntityTypeConfiguration<PersonelEntity>
    {
        public void Configure(EntityTypeBuilder<PersonelEntity> builder)
        {
            builder.ToTable("Personeller");

            builder.ConfigureBaseEntity();

            builder.Property(p => p.BirimId)
                .IsRequired();

            builder.HasIndex(p => p.BirimId);

            builder.Property(p => p.Ad)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.Soyad)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.Email)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(p => p.Telefon)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(p => p.Gorev)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.SorumluPersonel)
                .IsRequired(false)
                .HasMaxLength(100);
        }
    }
}
