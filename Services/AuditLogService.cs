using WebApplication1.DTOs.AuditLogs;
using WebApplication1.DTOs.Common;
using WebApplication1.Models;
using WebApplication1.Repositories;

namespace WebApplication1.Services
{
    public interface IAuditLogService
    {
        Task<PagedResult<AuditLogResponseDto>> GetAllAsync(
            AuditLogQueryParameters query);

        Task<AuditLogResponseDto?> GetByIdAsync(int id);
    }

    public class AuditLogService : IAuditLogService
    {
        private readonly IAuditLogRepository _repository;

        public AuditLogService(IAuditLogRepository repository)
        {
            _repository = repository;
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