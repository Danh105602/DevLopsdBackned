using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Repositories
{
    public class FieldRepository : IFieldRepository
    {
        private readonly AppDbContext _context;

        public FieldRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Field>> GetAllAsync()
        {
            return await _context.Fields
                .Include(f => f.Farm)
                .ToListAsync();
        }

        public async Task<Field?> GetByIdAsync(int id)
        {
            return await _context.Fields
                .Include(f => f.Farm)
                .Include(f => f.Plantings)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<Field> CreateAsync(Field field)
        {
            _context.Fields.Add(field);
            await _context.SaveChangesAsync();

            return field;
        }

        public async Task<Field?> UpdateAsync(int id, Field field)
        {
            var existing = await _context.Fields.FindAsync(id);

            if (existing == null)
                return null;

            existing.Name = field.Name;
            existing.FarmId = field.FarmId;
            existing.SoilType = field.SoilType;
            existing.Area = field.Area;
            existing.Description = field.Description;

            await _context.SaveChangesAsync();

            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var field = await _context.Fields.FindAsync(id);

            if (field == null)
                return false;

            _context.Fields.Remove(field);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}