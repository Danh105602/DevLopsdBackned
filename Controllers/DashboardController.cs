using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Authorization;
using WebApplication1.DTOs.Dashboard;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    [Authorize(Policy = Permissions.DashboardRead)]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _service;
        private readonly IOwnershipService _ownership;

        public DashboardController(IDashboardService service, IOwnershipService ownership)
        {
            _service = service;
            _ownership = ownership;
        }

        [HttpGet("overview")]
        public async Task<IActionResult> GetOverview(
            [FromQuery] int? farmId,
            [FromQuery] int? ownerId)
        {
            var (filter, error) = await BuildFilter(null, null, farmId, ownerId);
            if (error != null) return error;
            return Ok(await _service.GetOverviewAsync(filter!));
        }

        [HttpGet("harvests")]
        public async Task<IActionResult> GetHarvests(
            [FromQuery] DateOnly? from,
            [FromQuery] DateOnly? to,
            [FromQuery] int? farmId,
            [FromQuery] int? ownerId)
        {
            var (filter, error) = await BuildFilter(from, to, farmId, ownerId);
            if (error != null) return error;
            return Ok(await _service.GetHarvestsAsync(filter!));
        }

        [HttpGet("care")]
        public async Task<IActionResult> GetCare(
            [FromQuery] DateOnly? from,
            [FromQuery] DateOnly? to,
            [FromQuery] int? farmId,
            [FromQuery] int? ownerId)
        {
            var (filter, error) = await BuildFilter(from, to, farmId, ownerId);
            if (error != null) return error;
            return Ok(await _service.GetCareAsync(filter!));
        }

        [HttpGet("pests")]
        public async Task<IActionResult> GetPests(
            [FromQuery] DateOnly? from,
            [FromQuery] DateOnly? to,
            [FromQuery] int? farmId,
            [FromQuery] int? ownerId)
        {
            var (filter, error) = await BuildFilter(from, to, farmId, ownerId);
            if (error != null) return error;
            return Ok(await _service.GetPestsAsync(filter!));
        }

        [HttpGet("admin")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<DashboardAdminDto>> GetAdmin()
        {
            return Ok(await _service.GetAdminAsync());
        }

        private async Task<(DashboardFilter? Filter, IActionResult? Error)> BuildFilter(
            DateOnly? from,
            DateOnly? to,
            int? farmId,
            int? ownerId)
        {
            if (farmId is <= 0 || ownerId is <= 0)
                return (null, BadRequest(new { message = "FarmId and OwnerId must be positive." }));

            int? scopedOwnerId;
            if (User.IsInRole("Admin"))
            {
                scopedOwnerId = ownerId;
            }
            else
            {
                if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                    return (null, Unauthorized());

                if (ownerId.HasValue && ownerId.Value != userId)
                    return (null, Forbidden("Farmers can only view their own dashboard data."));

                if (farmId.HasValue && !await _ownership.OwnsFarmAsync(farmId.Value, userId))
                    return (null, Forbidden("You do not own the selected farm."));

                scopedOwnerId = userId;
            }

            var today = DashboardTime.Today;
            var startDate = from ?? today.AddYears(-1);
            var endDate = to ?? today;
            if (endDate < startDate || endDate == DateOnly.MaxValue)
                return (null, BadRequest(new { message = "The date range is invalid." }));

            return (new DashboardFilter(
                scopedOwnerId,
                farmId,
                startDate,
                endDate,
                DashboardTime.StartOfDayUtc(startDate),
                DashboardTime.StartOfDayUtc(endDate.AddDays(1))), null);
        }

        private ObjectResult Forbidden(string message) =>
            StatusCode(StatusCodes.Status403Forbidden, new { message });
    }
}
