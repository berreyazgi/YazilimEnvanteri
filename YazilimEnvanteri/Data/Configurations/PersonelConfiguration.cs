using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YazilimEnvanteri.Models.Entities;
using YazilimEnvanteri.Models.Identity;

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

            builder.Property(p => p.KullanıcıAdi)
                .IsRequired(false)
                .HasMaxLength(50);

            builder.HasIndex(p => p.KullanıcıAdi);
            builder.HasIndex(p => p.Email);

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

            // 1-1 with the login account; deleting the account just unlinks the staff record.
            builder.HasOne<ApplicationUser>()
                .WithOne()
                .HasForeignKey<PersonelEntity>(p => p.AppUserId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
