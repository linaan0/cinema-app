namespace CinemaApp.Notifications.Domain.Dto;

public record NotificationDto(string Id, string Type, string Content, string Status, DateTime? SentAt);
