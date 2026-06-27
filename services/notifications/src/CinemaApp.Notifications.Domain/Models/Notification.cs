using CinemaApp.Notifications.Domain.Common;

namespace CinemaApp.Notifications.Domain.Models;

public enum NotificationStatus
{
    Pending,
    Sent,
    Failed
}

public class Notification : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public string BookingId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public NotificationStatus Status { get; set; } = NotificationStatus.Pending;
    public DateTime? SentAt { get; set; }
}
