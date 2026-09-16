using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Net;
using YazilimEnvanteri.Models.Entities;

namespace YazilimEnvanteri.Data.Configurations
{
    public class ProjectConfiguraiton : IEntityTypeConfiguration<ProjeEntity>
    {
        public void Configure(EntityTypeBuilder<ProjeEntity> builder)
        {
            builder.ToTable("Proje", "Proje");

            builder.Property(x => x.ProjeAdi)
                .HasColumnType("varchar(50)")
                .IsRequired();

            builder.Property(x => x.ProjeHizmetAlani)
                .HasMaxLength(50)
                .IsRequired()
                .HasComment("Projenin hizmet alanını belirtir. Örneğin: Yazılım, Altyapı, Uygulama vb.")
                .HasColumnType("varchar(50)");

            builder.Property(x => x.ProjeAciklamasi)
                .HasMaxLength(500);
        }
    }
}
