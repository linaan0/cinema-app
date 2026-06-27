using CinemaApp.Notifications.Domain.Models;
using MongoDB.Driver;

namespace CinemaApp.Notifications.Repository;

public interface INotificationRepository : IMongoRepository<Notification>
{
}

public class NotificationRepository : MongoRepository<Notification>, INotificationRepository
{
    public NotificationRepository(IMongoDatabase database) : base(database, "notifications")
    {
    }
}
