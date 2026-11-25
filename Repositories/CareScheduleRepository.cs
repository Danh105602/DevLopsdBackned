using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Repositories
{
    public class CareScheduleRepository : ICareScheduleRepository
    {
        private readonly AppDbContext _context;

        public CareScheduleRepository(AppDbContext context) => _context = context;

        public Task<List<CareSchedule>> GetAllAsync() => _context.CareSchedules
            .AsNoTracking()
            .OrderBy(c => c.ScheduledDate)
            .ToListAsync();

        public Task<CareSchedule?> GetByIdAsync(int id) => _context.CareSchedules
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        public async Task<CareSchedule> CreateAsync(CareSchedule schedule)
        {
            _context.CareSchedules.Add(schedule);
            await _context.SaveChangesAsync();
            return schedule;
        }

        public async Task<CareSchedule?> UpdateAsync(int id, CareSchedule schedule)
        {
            var existing = await _context.CareSchedules.FindAsync(id);
            if (existing == null) return null;

            existing.TaskType = schedule.TaskType;
            existing.ScheduledDate = schedule.ScheduledDate;
            existing.Status = schedule.Status;
            existing.Notes = schedule.Notes;
            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> CompleteAsync(int id)
        {
            var existing = await _context.CareSchedules.FindAsync(id);
            if (existing == null || existing.Status == CareScheduleStatuses.Skipped)
                return false;

            if (existing.Status != CareScheduleStatuses.Completed)
            {
                existing.Status = CareScheduleStatuses.Completed;
                existing.CompletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _context.CareSchedules.FindAsync(id);
            if (existing == null) return false;

            _context.CareSchedules.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
