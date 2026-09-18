using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using YazilimEnvanteri.Models.Entities;

namespace YazilimEnvanteri.Data.Configurations
{
    public class TeknolojiConfiguration : IEntityTypeConfiguration<TeknolojiEntity>
    {
        public void Configure(EntityTypeBuilder<TeknolojiEntity> builder)
        {
            builder.ToTable("Teknolojiler");

            builder.ConfigureBaseEntity();

            builder.Property(t => t.EntegrasyonDurum)
                .IsRequired();

            builder.Property(t => t.Veritabani)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(t => t.FrontendTeknoloji)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(t => t.BackendTeknoloji)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(t => t.eimzaDurum)
                .IsRequired();
        }
    }
}
