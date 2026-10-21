using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // =========================
        // USERS
        // =========================
        public DbSet<User> Users { get; set; }

        // =========================
        // FARMS
        // =========================
        public DbSet<Farm> Farms { get; set; }

        // =========================
        // FIELDS
        // =========================
        public DbSet<Field> Fields { get; set; }

        // =========================
        // CROPS
        // =========================
        public DbSet<Crop> Crops { get; set; }

        // =========================
        // PLANTINGS
        // =========================
        public DbSet<Planting> Plantings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =========================
            // USER -> FARM
            // 1 User có nhiều Farm
            // =========================
            modelBuilder.Entity<Farm>()
                .HasOne(f => f.Owner)
                .WithMany(u => u.Farms)
                .HasForeignKey(f => f.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================
            // FARM -> FIELD
            // 1 Farm có nhiều Field
            // =========================
            modelBuilder.Entity<Field>()
                .HasOne(f => f.Farm)
                .WithMany(f => f.Fields)
                .HasForeignKey(f => f.FarmId)
                .OnDelete(DeleteBehavior.Cascade);

            // =========================
            // FIELD -> PLANTING
            // 1 Field có nhiều Planting
            // =========================
            modelBuilder.Entity<Planting>()
                .HasOne(p => p.Field)
                .WithMany(f => f.Plantings)
                .HasForeignKey(p => p.FieldId)
                .OnDelete(DeleteBehavior.Cascade);

            // =========================
            // CROP -> PLANTING
            // 1 Crop có nhiều Planting
            // =========================
            modelBuilder.Entity<Planting>()
                .HasOne(p => p.Crop)
                .WithMany(c => c.Plantings)
                .HasForeignKey(p => p.CropId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}