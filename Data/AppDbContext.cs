using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using System.Security.Claims;
using System.Text.Json;
using WebApplication1.Models;

namespace WebApplication1.Data
{
    public class AppDbContext : DbContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AppDbContext(
            DbContextOptions<AppDbContext> options,
            IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public DbSet<StoredFile> StoredFiles { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Farm> Farms { get; set; }
        public DbSet<Field> Fields { get; set; }
        public DbSet<Crop> Crops { get; set; }
        public DbSet<Planting> Plantings { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Harvest> Harvests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // USER -> FARM
            modelBuilder.Entity<Farm>()
                .HasOne(f => f.Owner)
                .WithMany(u => u.Farms)
                .HasForeignKey(f => f.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            // FARM -> FIELD
            modelBuilder.Entity<Field>()
                .HasOne(f => f.Farm)
                .WithMany(f => f.Fields)
                .HasForeignKey(f => f.FarmId)
                .OnDelete(DeleteBehavior.Cascade);

            // FIELD -> PLANTING
            modelBuilder.Entity<Planting>()
                .HasOne(p => p.Field)
                .WithMany(f => f.Plantings)
                .HasForeignKey(p => p.FieldId)
                .OnDelete(DeleteBehavior.Cascade);

            // CROP -> PLANTING
            modelBuilder.Entity<Planting>()
                .HasOne(p => p.Crop)
                .WithMany(c => c.Plantings)
                .HasForeignKey(p => p.CropId)
                .OnDelete(DeleteBehavior.Restrict);

            // REFRESH TOKEN
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.TokenHash)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(x => x.ReplacedByTokenHash)
                    .HasMaxLength(128);

                entity.HasIndex(x => x.TokenHash)
                    .IsUnique();

                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // PLANTING -> HARVEST (không sửa model Planting)
            modelBuilder.Entity<Harvest>(entity =>
            {
                entity.HasOne(h => h.Planting)
                    .WithMany()
                    .HasForeignKey(h => h.PlantingId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(h => h.PlantingId);
            });

            // AUDIT LOG (không có khóa ngoại, log tồn tại độc lập)
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasIndex(x => x.Timestamp);
                entity.HasIndex(x => x.UserId);
                entity.HasIndex(x => new { x.EntityName, x.EntityId });
            });
        }

        // ==========================================
        // AUTOMATIC AUDIT LOG
        // ==========================================

        private sealed record AuditEntry(
            object Entity,
            EntityState State,
            string EntityName,
            string? OldValues,
            string? NewValues);

        public override async Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            // Chụp dữ liệu TRƯỚC khi lưu
            var pending = CaptureAuditEntries();

            if (pending.Count == 0)
            {
                return await base.SaveChangesAsync(cancellationToken);
            }

            IDbContextTransaction? transaction = null;

            if (Database.CurrentTransaction == null)
            {
                transaction =
                    await Database.BeginTransactionAsync(cancellationToken);
            }

            try
            {
                // 1. Lưu dữ liệu nghiệp vụ (để có Id mới khi CREATE)
                var affectedRows =
                    await base.SaveChangesAsync(cancellationToken);

                // 2. Thông tin người thực hiện
                var httpContext = _httpContextAccessor.HttpContext;
                var user = httpContext?.User;

                var userIdClaim =
                    user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                int? userId = int.TryParse(userIdClaim, out var parsedId)
                    ? parsedId
                    : null;

                var username =
                    user?.FindFirst(ClaimTypes.Name)?.Value ?? "Anonymous";

                var role =
                    user?.FindFirst(ClaimTypes.Role)?.Value ?? "Unknown";

                var ipAddress =
                    httpContext?.Connection.RemoteIpAddress?.ToString();

                var now = DateTime.UtcNow;

                // 3. Tạo Audit Log
                var logs = pending.Select(item => new AuditLog
                {
                    UserId = userId,
                    Username = username,
                    Role = role,
                    Action = item.State switch
                    {
                        EntityState.Added => "CREATE",
                        EntityState.Modified => "UPDATE",
                        EntityState.Deleted => "DELETE",
                        _ => "UNKNOWN"
                    },
                    EntityName = item.EntityName,
                    EntityId = GetEntityId(item.Entity),
                    OldValues = item.OldValues,
                    NewValues = item.NewValues,
                    Timestamp = now,
                    IpAddress = ipAddress
                }).ToList();

                AuditLogs.AddRange(logs);

                // 4. Lưu nhật ký trong cùng transaction
                affectedRows +=
                    await base.SaveChangesAsync(cancellationToken);

                if (transaction != null)
                {
                    await transaction.CommitAsync(cancellationToken);
                }

                return affectedRows;
            }
            catch
            {
                if (transaction != null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                }

                throw;
            }
            finally
            {
                if (transaction != null)
                {
                    await transaction.DisposeAsync();
                }
            }
        }

        private List<AuditEntry> CaptureAuditEntries()
        {
            var result = new List<AuditEntry>();

            // ChangeTracker.Entries() tự gọi DetectChanges
            foreach (var entry in ChangeTracker.Entries().ToList())
            {
                if (entry.Entity is not (Farm or Field or Crop or Planting or Harvest))
                    continue;

                if (entry.State is not (EntityState.Added
                    or EntityState.Modified
                    or EntityState.Deleted))
                    continue;

                string? oldValues = null;
                string? newValues = null;

                switch (entry.State)
                {
                    case EntityState.Added:
                        newValues = ToJson(entry, useOriginal: false, onlyModified: false);
                        break;

                    case EntityState.Deleted:
                        oldValues = ToJson(entry, useOriginal: true, onlyModified: false);
                        break;

                    case EntityState.Modified:
                        oldValues = ToJson(entry, useOriginal: true, onlyModified: true);
                        newValues = ToJson(entry, useOriginal: false, onlyModified: true);

                        // Không có cột nào thực sự thay đổi -> bỏ qua
                        if (oldValues == null) continue;
                        break;
                }

                result.Add(new AuditEntry(
                    entry.Entity,
                    entry.State,
                    entry.Metadata.ClrType.Name,
                    oldValues,
                    newValues));
            }

            return result;
        }

        private static string? ToJson(
            EntityEntry entry,
            bool useOriginal,
            bool onlyModified)
        {
            var values = new Dictionary<string, object?>();

            foreach (var property in entry.Properties)
            {
                if (property.Metadata.IsPrimaryKey())
                    continue;

                if (onlyModified && !property.IsModified)
                    continue;

                values[property.Metadata.Name] = useOriginal
                    ? property.OriginalValue
                    : property.CurrentValue;
            }

            return values.Count == 0
                ? null
                : JsonSerializer.Serialize(values);
        }

        private static int GetEntityId(object entity) => entity switch
        {
            Farm farm => farm.Id,
            Field field => field.Id,
            Crop crop => crop.Id,
            Planting planting => planting.Id,
            Harvest harvest => harvest.Id,
            _ => 0
        };
    }
}