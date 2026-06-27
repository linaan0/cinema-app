using CinemaApp.Bookings.Domain.Common;

namespace CinemaApp.Bookings.Domain.Models;

public class Booking : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string ScreeningId { get; set; } = string.Empty;
    public List<string> SeatIds { get; set; } = new();
    public decimal TotalPrice { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Confirmed;
}

public enum BookingStatus
{
    Confirmed,
    Cancelled
}
