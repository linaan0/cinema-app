namespace CinemaApp.Bookings.Domain.Events;

// Published to RabbitMQ (queue "booking.confirmed") when a booking is created.
// The Notifications service consumes this to send a confirmation email.
public record BookingConfirmedEvent(
    string BookingId,
    string UserId,
    string UserEmail,
    string ScreeningId,
    List<string> SeatIds,
    decimal TotalPrice,
    DateTime ConfirmedAt);
