using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Repositories
{
    public class CropRepository : ICropRepository
    {
        private readonly AppDbContext _context;

        public CropRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Crop>> GetAllAsync()
        {
            return await _context.Crops
                .Include(c => c.Plantings)
                .ToListAsync();
        }

        public async Task<Crop?> GetByIdAsync(int id)
        {
            return await _context.Crops
                .Include(c => c.Plantings)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Crop> CreateAsync(Crop crop)
        {
            _context.Crops.Add(crop);
            await _context.SaveChangesAsync();

            return crop;
        }

        public async Task<Crop?> UpdateAsync(int id, Crop crop)
        {
            var existing = await _context.Crops.FindAsync(id);

            if (existing == null)
                return null;

            existing.Name = crop.Name;
            existing.Category = crop.Category;
            existing.Description = crop.Description;
            existing.GrowthDays = crop.GrowthDays;

            await _context.SaveChangesAsync();

            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var crop = await _context.Crops.FindAsync(id);

            if (crop == null)
                return false;

            _context.Crops.Remove(crop);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}