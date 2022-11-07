using Microsoft.EntityFrameworkCore;

namespace labelbox.Data
{
    public class DataContext : DbContext, IDataContext
    {
        public DataContext() { }
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<Asset> Assets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Asset>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).IsRequired().ValueGeneratedNever();
                entity.Property(e => e.Path).IsRequired();
                entity.Property(e => e.OnSuccessURL).IsRequired();
                entity.Property(e => e.OnStartURL).IsRequired();
                entity.Property(e => e.OnFailureURL).IsRequired();
            });
        }
    }
}
