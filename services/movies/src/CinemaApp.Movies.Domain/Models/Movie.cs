using CinemaApp.Movies.Domain.Common;

namespace CinemaApp.Movies.Domain.Models;

public class Movie : BaseEntity
{
    // Reference to TMDB (https://www.themoviedb.org) so the catalog can be enriched
    // with posters, descriptions, etc. without owning that data.
    public int? TmdbId { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Genres { get; set; } = new();
    public int RuntimeMinutes { get; set; }
    public DateTime ReleaseDate { get; set; }
    public string? PosterUrl { get; set; }
}
