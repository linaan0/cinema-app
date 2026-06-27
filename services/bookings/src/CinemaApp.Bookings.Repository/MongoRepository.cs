using System.Linq.Expressions;
using CinemaApp.Bookings.Domain.Common;
using MongoDB.Driver;

namespace CinemaApp.Bookings.Repository;

public interface IMongoRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(string id);
    Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null);
    Task<T> InsertAsync(T entity);
    Task ReplaceAsync(T entity);
    Task DeleteAsync(string id);
}

public abstract class MongoRepository<T> : IMongoRepository<T> where T : BaseEntity
{
    protected readonly IMongoCollection<T> Collection;

    protected MongoRepository(IMongoDatabase database, string collectionName)
    {
        Collection = database.GetCollection<T>(collectionName);
    }

    public async Task<T?> GetByIdAsync(string id) =>
        await Collection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null)
    {
        var predicate = filter ?? (_ => true);
        return await Collection.Find(predicate).ToListAsync();
    }

    public async Task<T> InsertAsync(T entity)
    {
        await Collection.InsertOneAsync(entity);
        return entity;
    }

    public async Task ReplaceAsync(T entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        await Collection.ReplaceOneAsync(x => x.Id == entity.Id, entity);
    }

    public async Task DeleteAsync(string id) =>
        await Collection.DeleteOneAsync(x => x.Id == id);
}
