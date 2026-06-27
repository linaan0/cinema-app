using CinemaApp.Auth.Domain.Models;
using MongoDB.Driver;

namespace CinemaApp.Auth.Repository;

public interface IUserRepository : IMongoRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
}

public class UserRepository : MongoRepository<User>, IUserRepository
{
    public UserRepository(IMongoDatabase database) : base(database, "users")
    {
        var indexKeys = Builders<User>.IndexKeys.Ascending(u => u.Email);
        var indexModel = new CreateIndexModel<User>(indexKeys, new CreateIndexOptions { Unique = true });
        Collection.Indexes.CreateOne(indexModel);
    }

    public async Task<User?> GetByEmailAsync(string email) =>
        await Collection.Find(u => u.Email == email).FirstOrDefaultAsync();
}
