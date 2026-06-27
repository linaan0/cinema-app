namespace CinemaApp.Bookings.Domain.Dto;

public record ScreeningDto(
    string Id,
    string MovieId,
    string HallId,
    DateTime StartTime,
    decimal BasePrice,
    int TotalSeats,
    int AvailableSeats);

public record SeatDto(string SeatId, string Row, int Number, string Status, bool LockedByMe);

public record SeatMapDto(string ScreeningId, List<SeatDto> Seats);

public record CreateBookingRequest(string ScreeningId, List<string> SeatIds);

public record BookingDto(string Id, string ScreeningId, List<string> SeatIds, decimal TotalPrice, string Status, DateTime CreatedAt);
