namespace CinemaApp.Movies.Domain.Dto;

public record MovieDto(
    string Id,
    int? TmdbId,
    string Title,
    string Description,
    List<string> Genres,
    int RuntimeMinutes,
    DateTime ReleaseDate,
    string? PosterUrl);

public record UpsertMovieRequest(
    int? TmdbId,
    string Title,
    string Description,
    List<string> Genres,
    int RuntimeMinutes,
    DateTime ReleaseDate,
    string? PosterUrl);
