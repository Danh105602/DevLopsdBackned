
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Repositories
{
    public class HarvestRepository : IHarvestRepository
    {
        private readonly AppDbContext _context;

        public HarvestRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Harvest>> GetAllAsync()
        {
            return await _context.Harvests
                .AsNoTracking()
                .OrderByDescending(h => h.HarvestDate)
                .ToListAsync();
        }

        public async Task<Harvest?> GetByIdAsync(int id)
        {
            return await _context.Harvests
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.Id == id);
        }

        public async Task<Harvest> CreateAsync(Harvest harvest)
        {
            _context.Harvests.Add(harvest);

            await _context.SaveChangesAsync();

            return harvest;
        }

        public async Task<Harvest?> UpdateAsync(int id, Harvest harvest)
        {
            var existing = await _context.Harvests
                .FirstOrDefaultAsync(h => h.Id == id);

            if (existing == null)
                return null;

            existing.HarvestDate = harvest.HarvestDate;
            existing.QuantityKg = harvest.QuantityKg;
            existing.Quality = harvest.Quality;
            existing.Notes = harvest.Notes;

            await _context.SaveChangesAsync();

            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var harvest = await _context.Harvests
                .FirstOrDefaultAsync(h => h.Id == id);

            if (harvest == null)
                return false;

            _context.Harvests.Remove(harvest);

            await _context.SaveChangesAsync();

            return true;
        }
    }
}
