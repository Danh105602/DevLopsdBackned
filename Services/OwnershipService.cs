using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;

namespace WebApplication1.Services
{
    public interface IOwnershipService
    {
        Task<bool> OwnsFarmAsync(int farmId, int userId);
        Task<bool> OwnsFieldAsync(int fieldId, int userId);
        Task<bool> OwnsPlantingAsync(int plantingId, int userId);
    }

    public class OwnershipService : IOwnershipService
    {
        private readonly AppDbContext _context;

        public OwnershipService(AppDbContext context)
        {
            _context = context;
        }

        public Task<bool> OwnsFarmAsync(int farmId, int userId)
        {
            return _context.Farms
                .AsNoTracking()
                .AnyAsync(f => f.Id == farmId && f.OwnerId == userId);
        }

        public Task<bool> OwnsFieldAsync(int fieldId, int userId)
        {
            return _context.Fields
                .AsNoTracking()
                .AnyAsync(f => f.Id == fieldId && f.Farm!.OwnerId == userId);
        }

        public Task<bool> OwnsPlantingAsync(int plantingId, int userId)
        {
            return _context.Plantings
                .AsNoTracking()
                .AnyAsync(p => p.Id == plantingId
                            && p.Field!.Farm!.OwnerId == userId);
        }
    }
}