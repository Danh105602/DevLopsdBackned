using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Repositories
{
    public class PestIncidentRepository : IPestIncidentRepository
    {
        private readonly AppDbContext _context;

        public PestIncidentRepository(AppDbContext context) => _context = context;

        public Task<List<PestIncident>> GetAllAsync() => _context.PestIncidents
            .AsNoTracking()
            .OrderByDescending(p => p.DetectedDate)
            .ToListAsync();

        public Task<PestIncident?> GetByIdAsync(int id) => _context.PestIncidents
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        public async Task<PestIncident> CreateAsync(PestIncident incident)
        {
            _context.PestIncidents.Add(incident);
            await _context.SaveChangesAsync();
            return incident;
        }

        public async Task<PestIncident?> UpdateAsync(int id, PestIncident incident)
        {
            var existing = await _context.PestIncidents.FindAsync(id);
            if (existing == null) return null;

            existing.PestName = incident.PestName;
            existing.Severity = incident.Severity;
            existing.DetectedDate = incident.DetectedDate;
            existing.Treatment = incident.Treatment;
            existing.Status = incident.Status;
            existing.ResolutionDate = incident.ResolutionDate;
            existing.Notes = incident.Notes;
            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _context.PestIncidents.FindAsync(id);
            if (existing == null) return false;

            _context.PestIncidents.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
