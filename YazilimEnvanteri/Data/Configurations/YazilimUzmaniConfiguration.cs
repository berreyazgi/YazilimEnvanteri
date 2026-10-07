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

            builder.Property(y => y.SorumluFirma)
                .IsRequired()
                .HasMaxLength(150);

            builder.HasOne<PersonelEntity>()
                .WithMany()
                .HasForeignKey(y => y.PersonelId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<BirimEntity>()
                .WithMany()
                .HasForeignKey(y => y.BirimId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<ProjeEntity>()
                .WithMany()
                .HasForeignKey(y => y.ProjeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
