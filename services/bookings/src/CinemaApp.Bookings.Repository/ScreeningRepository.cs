using CinemaApp.Bookings.Domain.Models;
using MongoDB.Driver;

namespace CinemaApp.Bookings.Repository;

public interface IScreeningRepository : IMongoRepository<Screening>
{
}

public class ScreeningRepository : MongoRepository<Screening>, IScreeningRepository
{
    public ScreeningRepository(IMongoDatabase database) : base(database, "screenings")
    {
    }
}
