using CinemaApp.Movies.Domain.Dto;
using CinemaApp.Movies.Domain.Models;
using CinemaApp.Movies.Repository;

namespace CinemaApp.Movies.Service;

public interface IMovieService
{
    Task<IEnumerable<MovieDto>> GetAllAsync(string? search = null);
    Task<MovieDto?> GetByIdAsync(string id);
    Task<MovieDto> CreateAsync(UpsertMovieRequest request);
    Task<MovieDto?> UpdateAsync(string id, UpsertMovieRequest request);
    Task<bool> DeleteAsync(string id);
}

public class MovieService : IMovieService
{
    private readonly IMovieRepository _repository;

    public MovieService(IMovieRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<MovieDto>> GetAllAsync(string? search = null)
    {
        var movies = string.IsNullOrWhiteSpace(search)
            ? await _repository.GetAllAsync()
            : await _repository.SearchByTitleAsync(search);

        return movies.Select(ToDto);
    }

    public async Task<MovieDto?> GetByIdAsync(string id)
    {
        var movie = await _repository.GetByIdAsync(id);
        return movie is null ? null : ToDto(movie);
    }

    public async Task<MovieDto> CreateAsync(UpsertMovieRequest request)
    {
        var movie = new Movie
        {
            TmdbId = request.TmdbId,
            Title = request.Title,
            Description = request.Description,
            Genres = request.Genres,
            RuntimeMinutes = request.RuntimeMinutes,
            ReleaseDate = request.ReleaseDate,
            PosterUrl = request.PosterUrl
        };

        await _repository.InsertAsync(movie);
        return ToDto(movie);
    }

    public async Task<MovieDto?> UpdateAsync(string id, UpsertMovieRequest request)
    {
        var movie = await _repository.GetByIdAsync(id);
        if (movie is null)
        {
            return null;
        }

        movie.TmdbId = request.TmdbId;
        movie.Title = request.Title;
        movie.Description = request.Description;
        movie.Genres = request.Genres;
        movie.RuntimeMinutes = request.RuntimeMinutes;
        movie.ReleaseDate = request.ReleaseDate;
        movie.PosterUrl = request.PosterUrl;

        await _repository.ReplaceAsync(movie);
        return ToDto(movie);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var movie = await _repository.GetByIdAsync(id);
        if (movie is null)
        {
            return false;
        }

        await _repository.DeleteAsync(id);
        return true;
    }

    private static MovieDto ToDto(Movie movie) => new(
        movie.Id,
        movie.TmdbId,
        movie.Title,
        movie.Description,
        movie.Genres,
        movie.RuntimeMinutes,
        movie.ReleaseDate,
        movie.PosterUrl);
}
