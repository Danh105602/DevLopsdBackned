using WebApplication1.DTOs.AuditLogs;
using WebApplication1.DTOs.Common;
using WebApplication1.Models;
using WebApplication1.Repositories;
using System.Security.Claims;

namespace WebApplication1.Services
{
    public interface IAuditLogService
    {
        Task<PagedResult<AuditLogResponseDto>> GetAllAsync(
            AuditLogQueryParameters query);

        Task<AuditLogResponseDto?> GetByIdAsync(int id);

        Task RecordReadAsync(string entityName, int entityId, bool isList = false);
    }

    public class AuditLogService : IAuditLogService
    {
        private readonly IAuditLogRepository _repository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditLogService(IAuditLogRepository repository, IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _httpContextAccessor = httpContextAccessor;
        }

        public Task RecordReadAsync(string entityName, int entityId, bool isList = false)
        {
            var context = _httpContextAccessor.HttpContext;
            var user = context?.User;
            var userId = int.TryParse(user?.FindFirstValue(ClaimTypes.NameIdentifier), out var parsedId)
                ? parsedId
                : (int?)null;

            return _repository.AddAsync(new AuditLog
            {
                UserId = userId,
                Username = user?.FindFirstValue(ClaimTypes.Name) ?? "Anonymous",
                Role = user?.FindFirstValue(ClaimTypes.Role) ?? "Unknown",
                Action = isList ? "READ_LIST" : "READ",
                EntityName = entityName,
                EntityId = entityId,
                Timestamp = DateTime.UtcNow,
                IpAddress = context?.Connection.RemoteIpAddress?.ToString()
            });
        }

        public async Task<PagedResult<AuditLogResponseDto>> GetAllAsync(
            AuditLogQueryParameters query)
        {
            var (items, total) = await _repository.GetAllAsync(query);

            return new PagedResult<AuditLogResponseDto>(
                items.Select(Map).ToList(),
                total,
                query.PageNumber,
                query.PageSize);
        }

        public async Task<AuditLogResponseDto?> GetByIdAsync(int id)
        {
            var log = await _repository.GetByIdAsync(id);
            return log == null ? null : Map(log);
        }

        private static AuditLogResponseDto Map(AuditLog log)
        {
            return new AuditLogResponseDto
            {
                Id = log.Id,
                UserId = log.UserId,
                Username = log.Username,
                Role = log.Role,
                Action = log.Action,
                EntityName = log.EntityName,
                EntityId = log.EntityId,
                OldValues = log.OldValues,
                NewValues = log.NewValues,
                Timestamp = log.Timestamp,
                IpAddress = log.IpAddress
            };
        }
    }
}
