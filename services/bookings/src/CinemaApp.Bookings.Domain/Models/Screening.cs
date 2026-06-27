using CinemaApp.Bookings.Domain.Common;

namespace CinemaApp.Bookings.Domain.Models;

public class Screening : BaseEntity
{
    public string MovieId { get; set; } = string.Empty;
    public string HallId { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public decimal BasePrice { get; set; }
    public List<Seat> Seats { get; set; } = new();
}

public class Seat
{
    public string SeatId { get; set; } = string.Empty; // e.g. "A1"
    public string Row { get; set; } = string.Empty;
    public int Number { get; set; }

    // NOTE: there is intentionally NO "Locked" status persisted here.
    // Temporary seat locks live only in Redis (with a TTL). Mongo only
    // ever sees Available -> Booked. This keeps the source of truth simple
    // and means an expired lock "heals" automatically without any cleanup job.
    public SeatStatus Status { get; set; } = SeatStatus.Available;
}

public enum SeatStatus
{
    Available,
    Booked
}
