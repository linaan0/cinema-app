namespace CinemaApp.Bookings.Service;

// Abstraction over "tell connected clients a seat's status changed".
// The real implementation (SignalR) lives in the Api project so this
// Service layer does not need to depend on ASP.NET Core SignalR types.
public interface ISeatStatusNotifier
{
    Task NotifySeatStatusChangedAsync(string screeningId, string seatId, string status);
}
