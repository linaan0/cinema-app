using CinemaApp.Bookings.Domain.Models;
using MongoDB.Driver;

namespace CinemaApp.Bookings.Repository;

public interface IBookingRepository : IMongoRepository<Booking>
{
}

public class BookingRepository : MongoRepository<Booking>, IBookingRepository
{
    public BookingRepository(IMongoDatabase database) : base(database, "bookings")
    {
    }
}
