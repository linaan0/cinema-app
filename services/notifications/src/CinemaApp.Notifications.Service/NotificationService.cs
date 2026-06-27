using CinemaApp.Notifications.Domain.Dto;
using CinemaApp.Notifications.Domain.Events;
using CinemaApp.Notifications.Domain.Models;
using CinemaApp.Notifications.Repository;
using Microsoft.Extensions.Logging;

namespace CinemaApp.Notifications.Service;

public interface INotificationService
{
    Task HandleBookingConfirmedAsync(BookingConfirmedEvent evt);
    Task<IEnumerable<NotificationDto>> GetMyNotificationsAsync(string userId);
}

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repository;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(INotificationRepository repository, ILogger<NotificationService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task HandleBookingConfirmedAsync(BookingConfirmedEvent evt)
    {
        var seatList = string.Join(", ", evt.SeatIds);
        var notification = new Notification
        {
            UserId = evt.UserId,
            BookingId = evt.BookingId,
            Type = "BookingConfirmation",
            Content = $"Your booking {evt.BookingId} for seats {seatList} is confirmed. Total: {evt.TotalPrice:C}.",
            Status = NotificationStatus.Pending
        };

        await _repository.InsertAsync(notification);

        // Mock email send - swap this for a real SMTP/SES/SendGrid client.
        _logger.LogInformation("Sending booking confirmation email to {Email}: {Content}", evt.UserEmail, notification.Content);

        notification.Status = NotificationStatus.Sent;
        notification.SentAt = DateTime.UtcNow;
        await _repository.ReplaceAsync(notification);
    }

    public async Task<IEnumerable<NotificationDto>> GetMyNotificationsAsync(string userId)
    {
        var notifications = await _repository.GetAllAsync(n => n.UserId == userId);
        return notifications
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NotificationDto(n.Id, n.Type, n.Content, n.Status.ToString(), n.SentAt));
    }
}
