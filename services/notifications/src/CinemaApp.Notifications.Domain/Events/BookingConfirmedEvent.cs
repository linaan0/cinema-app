namespace CinemaApp.Notifications.Domain.Events;

// Mirrors the event published by the Bookings service on the
// "booking.confirmed" RabbitMQ queue. Each service keeps its own copy of
// the contract on purpose - it avoids a shared library coupling two
// independently-deployable services together.
public record BookingConfirmedEvent(
    string BookingId,
    string UserId,
    string UserEmail,
    string ScreeningId,
    List<string> SeatIds,
    decimal TotalPrice,
    DateTime ConfirmedAt);
