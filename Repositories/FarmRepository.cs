using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Repositories
{
    public interface IFarmRepository
    {
        Task<List<Farm>> GetAllAsync();
        Task<Farm?> GetByIdAsync(int id);
        Task<Farm> CreateAsync(Farm farm);
        Task<Farm?> UpdateAsync(int id, Farm farm);
        Task<bool> DeleteAsync(int id);
    }

    public class FarmRepository : IFarmRepository
    {
        private readonly AppDbContext _context;

        public FarmRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Farm>> GetAllAsync()
        {
            return await _context.Farms
                .Include(f => f.Owner)
                .ToListAsync();
        }

        public async Task<Farm?> GetByIdAsync(int id)
        {
            return await _context.Farms
                .Include(f => f.Owner)
                .Include(f => f.Fields)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<Farm> CreateAsync(Farm farm)
        {
            _context.Farms.Add(farm);
            await _context.SaveChangesAsync();
            return farm;
        }

        public async Task<Farm?> UpdateAsync(int id, Farm farm)
        {
            var existingFarm = await _context.Farms.FindAsync(id);

            if (existingFarm == null)
                return null;

            existingFarm.Name = farm.Name;
            existingFarm.Location = farm.Location;
            existingFarm.Area = farm.Area;
            existingFarm.OwnerId = farm.OwnerId;

            await _context.SaveChangesAsync();

            return existingFarm;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var farm = await _context.Farms.FindAsync(id);

            if (farm == null)
                return false;

            _context.Farms.Remove(farm);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}