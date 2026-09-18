using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YazilimEnvanteri.Models.Entities;

namespace YazilimEnvanteri.Data.Configurations
{
    internal static class BaseEntityConfigurationExtensions
    {
        internal static void ConfigureBaseEntity<TEntity>(this EntityTypeBuilder<TEntity> builder)
            where TEntity : BaseEntity
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            builder.Property(e => e.OlusturmaTarihi)
                .IsRequired()
                .HasColumnType("timestamp with time zone");

            builder.Property(e => e.GuncellemeTarihi)
                .IsRequired(false)
                .HasColumnType("timestamp with time zone");
        }
    }
}
