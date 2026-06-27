using Microsoft.AspNetCore.SignalR;

namespace CinemaApp.Bookings.Api.Hubs;

// Clients connect here and join a "screening:{id}" group to receive
// live SeatStatusChanged events while browsing the seat map.
public class SeatAvailabilityHub : Hub
{
    public Task JoinScreening(string screeningId) =>
        Groups.AddToGroupAsync(Context.ConnectionId, GroupName(screeningId));

    public Task LeaveScreening(string screeningId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(screeningId));

    public static string GroupName(string screeningId) => $"screening:{screeningId}";
}
