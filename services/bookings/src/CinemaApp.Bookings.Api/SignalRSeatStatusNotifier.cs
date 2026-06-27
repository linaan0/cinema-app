using CinemaApp.Bookings.Api.Hubs;
using CinemaApp.Bookings.Service;
using Microsoft.AspNetCore.SignalR;

namespace CinemaApp.Bookings.Api;

public class SignalRSeatStatusNotifier : ISeatStatusNotifier
{
    private readonly IHubContext<SeatAvailabilityHub> _hubContext;

    public SignalRSeatStatusNotifier(IHubContext<SeatAvailabilityHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task NotifySeatStatusChangedAsync(string screeningId, string seatId, string status) =>
        _hubContext.Clients.Group(SeatAvailabilityHub.GroupName(screeningId))
            .SendAsync("SeatStatusChanged", new { seatId, status });
}
