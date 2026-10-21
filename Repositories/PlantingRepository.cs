using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Repositories
{
    public class PlantingRepository : IPlantingRepository
    {
        private readonly AppDbContext _context;

        public PlantingRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Planting>> GetAllAsync()
        {
            return await _context.Plantings
                .Include(p => p.Field)
                .Include(p => p.Crop)
                .ToListAsync();
        }

        public async Task<Planting?> GetByIdAsync(int id)
        {
            return await _context.Plantings
                .Include(p => p.Field)
                .Include(p => p.Crop)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Planting> CreateAsync(Planting planting)
        {
            _context.Plantings.Add(planting);
            await _context.SaveChangesAsync();

            return planting;
        }

        public async Task<Planting?> UpdateAsync(int id, Planting planting)
        {
            var existing = await _context.Plantings.FindAsync(id);

            if (existing == null)
                return null;

            existing.FieldId = planting.FieldId;
            existing.CropId = planting.CropId;
            existing.PlantingDate = planting.PlantingDate;
            existing.ExpectedHarvestDate = planting.ExpectedHarvestDate;
            existing.Status = planting.Status;

            await _context.SaveChangesAsync();

            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var planting = await _context.Plantings.FindAsync(id);

            if (planting == null)
                return false;

            _context.Plantings.Remove(planting);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}