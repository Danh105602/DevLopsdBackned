using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.DTOs.AuditLogs;
using WebApplication1.Models;

namespace WebApplication1.Repositories
{
    public interface IAuditLogRepository
    {
        Task<(List<AuditLog> Items, int TotalCount)> GetAllAsync(
            AuditLogQueryParameters query);

        Task<AuditLog?> GetByIdAsync(int id);

        Task AddAsync(AuditLog log);
    }

    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly AppDbContext _context;

        public AuditLogRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(List<AuditLog> Items, int TotalCount)> GetAllAsync(
            AuditLogQueryParameters query)
        {
            var logs = _context.AuditLogs.AsNoTracking().AsQueryable();

            if (query.UserId.HasValue)
                logs = logs.Where(x => x.UserId == query.UserId.Value);

            if (!string.IsNullOrWhiteSpace(query.Action))
            {
                var action = query.Action.Trim().ToUpper();
                logs = logs.Where(x => x.Action == action);
            }

            if (!string.IsNullOrWhiteSpace(query.EntityName))
            {
                var entityName = query.EntityName.Trim();
                logs = logs.Where(x => x.EntityName == entityName);
            }

            if (query.EntityId.HasValue)
                logs = logs.Where(x => x.EntityId == query.EntityId.Value);

            if (query.From.HasValue)
            {
                var from = query.From.Value.Date;
                logs = logs.Where(x => x.Timestamp >= from);
            }

            if (query.To.HasValue)
            {
                var toExclusive = query.To.Value.Date.AddDays(1);
                logs = logs.Where(x => x.Timestamp < toExclusive);
            }

            var total = await logs.CountAsync();

            var items = await logs
                .OrderByDescending(x => x.Timestamp)
                .ThenByDescending(x => x.Id)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return (items, total);
        }

        public async Task<AuditLog?> GetByIdAsync(int id)
        {
            return await _context.AuditLogs
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task AddAsync(AuditLog log)
        {
            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}
