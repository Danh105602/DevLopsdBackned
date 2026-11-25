using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;

namespace WebApplication1.Hubs;

[Authorize]
public class FarmHub : Hub
{
    private readonly AppDbContext _context;

    public FarmHub(AppDbContext context) => _context = context;

    public async Task JoinFarm(int farmId)
    {
        if (farmId <= 0) throw new HubException("Invalid farm id.");

        var exists = await _context.Farms.AsNoTracking().AnyAsync(f => f.Id == farmId);
        if (!exists) throw new HubException("Farm not found.");

        if (!Context.User!.IsInRole("Admin"))
        {
            var userIdText = Context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdText, out var userId) ||
                !await _context.Farms.AsNoTracking().AnyAsync(f => f.Id == farmId && f.OwnerId == userId))
                throw new HubException("You do not have access to this farm.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, FarmGroup(farmId));
    }

    public Task LeaveFarm(int farmId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, FarmGroup(farmId));

    public static string FarmGroup(int farmId) => $"farm:{farmId}";
}
