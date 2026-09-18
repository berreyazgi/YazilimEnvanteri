using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YazilimEnvanteri.Models.Entities;

namespace YazilimEnvanteri.Data.Configurations
{
    public class BirimConfiguration : IEntityTypeConfiguration<BirimEntity>
    {
        public void Configure(EntityTypeBuilder<BirimEntity> builder)
        {
            builder.ToTable("Birimler");

            builder.ConfigureBaseEntity();

            builder.Property(b => b.Birim)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(b => b.UstBirim)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(b => b.AltBirim)
                .IsRequired()
                .HasMaxLength(150);
 
            builder.HasOne<YazilimUzmaniEntity>()
                .WithMany()
                .HasForeignKey(b => b.YazilimUzmaniId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}