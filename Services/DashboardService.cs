using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.DTOs.Dashboard;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public sealed record DashboardFilter(
        int? OwnerId,
        int? FarmId,
        DateOnly From,
        DateOnly To,
        DateTime FromUtc,
        DateTime ToExclusiveUtc);

    public static class DashboardTime
    {
        private static readonly TimeZoneInfo VietnamTimeZone = GetVietnamTimeZone();

        public static DateOnly Today => DateOnly.FromDateTime(
            TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, VietnamTimeZone));

        public static DateTime StartOfDayUtc(DateOnly date)
        {
            var localStart = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
            return TimeZoneInfo.ConvertTimeToUtc(localStart, VietnamTimeZone);
        }

        private static TimeZoneInfo GetVietnamTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(
                    OperatingSystem.IsWindows() ? "SE Asia Standard Time" : "Asia/Ho_Chi_Minh");
            }
            catch (TimeZoneNotFoundException)
            {
                return TimeZoneInfo.FindSystemTimeZoneById(
                    OperatingSystem.IsWindows() ? "Asia/Ho_Chi_Minh" : "SE Asia Standard Time");
            }
        }
    }

    public interface IDashboardService
    {
        Task<DashboardOverviewDto> GetOverviewAsync(DashboardFilter filter);
        Task<DashboardHarvestDto> GetHarvestsAsync(DashboardFilter filter);
        Task<DashboardCareDto> GetCareAsync(DashboardFilter filter);
        Task<DashboardPestDto> GetPestsAsync(DashboardFilter filter);
        Task<DashboardAdminDto> GetAdminAsync();
    }

    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;

        public DashboardService(AppDbContext context) => _context = context;

        public async Task<DashboardOverviewDto> GetOverviewAsync(DashboardFilter filter)
        {
            var statuses = await Plantings(filter)
                .GroupBy(p => p.Status)
                .Select(g => new DashboardCountDto { Name = g.Key, Count = g.Count() })
                .OrderBy(x => x.Name)
                .ToListAsync();

            return new DashboardOverviewDto
            {
                FarmCount = await Farms(filter).CountAsync(),
                FieldCount = await Fields(filter).CountAsync(),
                PlantingCount = await Plantings(filter).CountAsync(),
                PlantingsByStatus = statuses
            };
        }

        public async Task<DashboardHarvestDto> GetHarvestsAsync(DashboardFilter filter)
        {
            var harvests = Harvests(filter);
            var byMonth = await harvests
                .GroupBy(h => new
                {
                    Year = h.HarvestDate.AddHours(7).Year,
                    Month = h.HarvestDate.AddHours(7).Month
                })
                .Select(g => new DashboardMonthHarvestDto
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    QuantityKg = g.Sum(h => h.QuantityKg)
                })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync();

            var byCrop = await harvests
                .GroupBy(h => new { h.Planting!.CropId, CropName = h.Planting.Crop!.Name })
                .Select(g => new DashboardCropHarvestDto
                {
                    CropId = g.Key.CropId,
                    CropName = g.Key.CropName,
                    QuantityKg = g.Sum(h => h.QuantityKg)
                })
                .OrderBy(x => x.CropName)
                .ToListAsync();

            var byFarm = await harvests
                .GroupBy(h => new { FarmId = h.Planting!.Field!.FarmId, FarmName = h.Planting.Field!.Farm!.Name })
                .Select(g => new DashboardFarmHarvestDto
                {
                    FarmId = g.Key.FarmId,
                    FarmName = g.Key.FarmName,
                    QuantityKg = g.Sum(h => h.QuantityKg)
                })
                .OrderBy(x => x.FarmName)
                .ToListAsync();

            var timing = await harvests
                .Where(h => h.Planting!.ExpectedHarvestDate.HasValue)
                .Select(h => EF.Functions.DateDiffDay(
                    h.Planting!.ExpectedHarvestDate!.Value.AddHours(7),
                    h.HarvestDate.AddHours(7)))
                .GroupBy(_ => 1)
                .Select(g => new DashboardHarvestTimingDto
                {
                    ComparedHarvestCount = g.Count(),
                    EarlyCount = g.Count(days => days < 0),
                    OnTimeCount = g.Count(days => days == 0),
                    LateCount = g.Count(days => days > 0),
                    AverageVarianceDays = g.Average(days => (double)days)
                })
                .FirstOrDefaultAsync();

            var missingExpectedDateCount = await harvests
                .CountAsync(h => !h.Planting!.ExpectedHarvestDate.HasValue);

            return new DashboardHarvestDto
            {
                From = filter.From,
                To = filter.To,
                HarvestCount = await harvests.CountAsync(),
                TotalQuantityKg = await harvests.SumAsync(h => (double?)h.QuantityKg) ?? 0d,
                ByMonth = byMonth,
                ByCrop = byCrop,
                ByFarm = byFarm,
                ExpectedVsActual = timing ?? new DashboardHarvestTimingDto(),
            }.AlsoSetMissingExpectedDates(missingExpectedDateCount);
        }

        public async Task<DashboardCareDto> GetCareAsync(DashboardFilter filter)
        {
            var schedules = CareSchedules(filter);
            var todayStart = DashboardTime.StartOfDayUtc(DashboardTime.Today);
            var nextWeekStart = DashboardTime.StartOfDayUtc(DashboardTime.Today.AddDays(7));
            var periodSchedules = schedules.Where(c => c.ScheduledDate >= filter.FromUtc && c.ScheduledDate < filter.ToExclusiveUtc);

            var scheduledCount = await periodSchedules.CountAsync();
            var completedCount = await periodSchedules.CountAsync(c => c.Status == CareScheduleStatuses.Completed);

            return new DashboardCareDto
            {
                From = filter.From,
                To = filter.To,
                OverdueCount = await schedules.CountAsync(c => c.Status == CareScheduleStatuses.Pending && c.ScheduledDate < todayStart),
                UpcomingNext7DaysCount = await schedules.CountAsync(c => c.Status == CareScheduleStatuses.Pending && c.ScheduledDate >= todayStart && c.ScheduledDate < nextWeekStart),
                ScheduledInPeriodCount = scheduledCount,
                CompletedInPeriodCount = completedCount,
                CompletionRatePercent = scheduledCount == 0 ? 0 : completedCount * 100d / scheduledCount
            };
        }

        public async Task<DashboardPestDto> GetPestsAsync(DashboardFilter filter)
        {
            var incidents = PestIncidents(filter)
                .Where(p => p.DetectedDate >= filter.FromUtc && p.DetectedDate < filter.ToExclusiveUtc)
                .Where(p => p.Status != PestIncidentStatuses.Resolved);

            return new DashboardPestDto
            {
                From = filter.From,
                To = filter.To,
                ActiveCount = await incidents.CountAsync(),
                HighSeverityActiveCount = await incidents.CountAsync(p => p.Severity == PestIncidentSeverities.High),
                ActiveBySeverity = await incidents
                    .GroupBy(p => p.Severity)
                    .Select(g => new DashboardCountDto { Name = g.Key, Count = g.Count() })
                    .OrderBy(x => x.Name)
                    .ToListAsync()
            };
        }

        public async Task<DashboardAdminDto> GetAdminAsync()
        {
            var today = DashboardTime.Today;
            var start = DashboardTime.StartOfDayUtc(today);
            var end = DashboardTime.StartOfDayUtc(today.AddDays(1));
            var todayLogs = _context.AuditLogs.AsNoTracking()
                .Where(a => a.Timestamp >= start && a.Timestamp < end);

            return new DashboardAdminDto
            {
                TotalUserCount = await _context.Users.CountAsync(),
                UsersByRole = await _context.Users.AsNoTracking()
                    .GroupBy(u => u.Role)
                    .Select(g => new DashboardCountDto { Name = g.Key, Count = g.Count() })
                    .OrderBy(x => x.Name)
                    .ToListAsync(),
                ActivityDate = today,
                AuditActionsToday = await todayLogs.CountAsync(),
                ActivityByUser = await todayLogs
                    .GroupBy(a => new { a.UserId, a.Username, a.Role })
                    .Select(g => new DashboardUserActivityDto
                    {
                        UserId = g.Key.UserId,
                        Username = g.Key.Username,
                        Role = g.Key.Role,
                        ActionCount = g.Count()
                    })
                    .OrderByDescending(x => x.ActionCount)
                    .ThenBy(x => x.Username)
                    .ToListAsync()
            };
        }

        private IQueryable<Farm> Farms(DashboardFilter filter)
        {
            var query = _context.Farms.AsNoTracking();
            if (filter.OwnerId.HasValue) query = query.Where(f => f.OwnerId == filter.OwnerId.Value);
            if (filter.FarmId.HasValue) query = query.Where(f => f.Id == filter.FarmId.Value);
            return query;
        }

        private IQueryable<Field> Fields(DashboardFilter filter)
        {
            var query = _context.Fields.AsNoTracking();
            if (filter.OwnerId.HasValue) query = query.Where(f => f.Farm!.OwnerId == filter.OwnerId.Value);
            if (filter.FarmId.HasValue) query = query.Where(f => f.FarmId == filter.FarmId.Value);
            return query;
        }

        private IQueryable<Planting> Plantings(DashboardFilter filter)
        {
            var query = _context.Plantings.AsNoTracking();
            if (filter.OwnerId.HasValue) query = query.Where(p => p.Field!.Farm!.OwnerId == filter.OwnerId.Value);
            if (filter.FarmId.HasValue) query = query.Where(p => p.Field!.FarmId == filter.FarmId.Value);
            return query;
        }

        private IQueryable<Harvest> Harvests(DashboardFilter filter)
        {
            var query = _context.Harvests.AsNoTracking()
                .Where(h => h.HarvestDate >= filter.FromUtc && h.HarvestDate < filter.ToExclusiveUtc);
            if (filter.OwnerId.HasValue) query = query.Where(h => h.Planting!.Field!.Farm!.OwnerId == filter.OwnerId.Value);
            if (filter.FarmId.HasValue) query = query.Where(h => h.Planting!.Field!.FarmId == filter.FarmId.Value);
            return query;
        }

        private IQueryable<CareSchedule> CareSchedules(DashboardFilter filter)
        {
            var query = _context.CareSchedules.AsNoTracking();
            if (filter.OwnerId.HasValue) query = query.Where(c => c.Planting!.Field!.Farm!.OwnerId == filter.OwnerId.Value);
            if (filter.FarmId.HasValue) query = query.Where(c => c.Planting!.Field!.FarmId == filter.FarmId.Value);
            return query;
        }

        private IQueryable<PestIncident> PestIncidents(DashboardFilter filter)
        {
            var query = _context.PestIncidents.AsNoTracking();
            if (filter.OwnerId.HasValue) query = query.Where(p => p.Planting!.Field!.Farm!.OwnerId == filter.OwnerId.Value);
            if (filter.FarmId.HasValue) query = query.Where(p => p.Planting!.Field!.FarmId == filter.FarmId.Value);
            return query;
        }
    }

    internal static class DashboardDtoExtensions
    {
        public static DashboardHarvestDto AlsoSetMissingExpectedDates(this DashboardHarvestDto dto, int count)
        {
            dto.ExpectedVsActual.MissingExpectedDateCount = count;
            return dto;
        }
    }
}
