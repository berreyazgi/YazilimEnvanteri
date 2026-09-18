using System.Reflection;
using Microsoft.EntityFrameworkCore;
using YazilimEnvanteri.Models.Entities;

namespace YazilimEnvanteri.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<ProjeEntity> Projeler => Set<ProjeEntity>();
        public DbSet<BirimEntity> Birimler => Set<BirimEntity>();
        public DbSet<PersonelEntity> Personeller => Set<PersonelEntity>();
        public DbSet<TeknolojiEntity> Teknolojiler => Set<TeknolojiEntity>();
        public DbSet<YazilimUzmaniEntity> YazilimUzmanlari => Set<YazilimUzmaniEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries<BaseEntity>();

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.OlusturmaTarihi = DateTime.UtcNow;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.GuncellemeTarihi = DateTime.UtcNow;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
