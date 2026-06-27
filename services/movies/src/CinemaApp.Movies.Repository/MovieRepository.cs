using CinemaApp.Movies.Domain.Models;
using MongoDB.Driver;

namespace CinemaApp.Movies.Repository;

public interface IMovieRepository : IMongoRepository<Movie>
{
    Task<IEnumerable<Movie>> SearchByTitleAsync(string query);
}

public class MovieRepository : MongoRepository<Movie>, IMovieRepository
{
    public MovieRepository(IMongoDatabase database) : base(database, "movies")
    {
    }

    public async Task<IEnumerable<Movie>> SearchByTitleAsync(string query)
    {
        var filter = Builders<Movie>.Filter.Regex(m => m.Title, new MongoDB.Bson.BsonRegularExpression(query, "i"));
        return await Collection.Find(filter).ToListAsync();
    }
}
