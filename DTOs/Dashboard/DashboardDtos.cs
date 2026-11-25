namespace WebApplication1.DTOs.Dashboard
{
    public class DashboardCountDto
    {
        public string Name { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class DashboardOverviewDto
    {
        public int FarmCount { get; set; }
        public int FieldCount { get; set; }
        public int PlantingCount { get; set; }
        public List<DashboardCountDto> PlantingsByStatus { get; set; } = [];
    }

    public class DashboardMonthHarvestDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public double QuantityKg { get; set; }
    }

    public class DashboardCropHarvestDto
    {
        public int CropId { get; set; }
        public string CropName { get; set; } = string.Empty;
        public double QuantityKg { get; set; }
    }

    public class DashboardFarmHarvestDto
    {
        public int FarmId { get; set; }
        public string FarmName { get; set; } = string.Empty;
        public double QuantityKg { get; set; }
    }

    public class DashboardHarvestTimingDto
    {
        public int ComparedHarvestCount { get; set; }
        public int EarlyCount { get; set; }
        public int OnTimeCount { get; set; }
        public int LateCount { get; set; }
        public int MissingExpectedDateCount { get; set; }
        public double? AverageVarianceDays { get; set; }
    }

    public class DashboardHarvestDto
    {
        public DateOnly From { get; set; }
        public DateOnly To { get; set; }
        public int HarvestCount { get; set; }
        public double TotalQuantityKg { get; set; }
        public List<DashboardMonthHarvestDto> ByMonth { get; set; } = [];
        public List<DashboardCropHarvestDto> ByCrop { get; set; } = [];
        public List<DashboardFarmHarvestDto> ByFarm { get; set; } = [];
        public DashboardHarvestTimingDto ExpectedVsActual { get; set; } = new();
    }

    public class DashboardCareDto
    {
        public DateOnly From { get; set; }
        public DateOnly To { get; set; }
        public int OverdueCount { get; set; }
        public int UpcomingNext7DaysCount { get; set; }
        public int ScheduledInPeriodCount { get; set; }
        public int CompletedInPeriodCount { get; set; }
        public double CompletionRatePercent { get; set; }
    }

    public class DashboardPestDto
    {
        public DateOnly From { get; set; }
        public DateOnly To { get; set; }
        public int ActiveCount { get; set; }
        public int HighSeverityActiveCount { get; set; }
        public List<DashboardCountDto> ActiveBySeverity { get; set; } = [];
    }

    public class DashboardAdminDto
    {
        public int TotalUserCount { get; set; }
        public List<DashboardCountDto> UsersByRole { get; set; } = [];
        public DateOnly ActivityDate { get; set; }
        public int AuditActionsToday { get; set; }
        public List<DashboardUserActivityDto> ActivityByUser { get; set; } = [];
    }

    public class DashboardUserActivityDto
    {
        public int? UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int ActionCount { get; set; }
    }
}
