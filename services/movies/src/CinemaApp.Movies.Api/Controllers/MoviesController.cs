using CinemaApp.Movies.Domain.Dto;
using CinemaApp.Movies.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaApp.Movies.Api.Controllers;

[ApiController]
[Route("api/movies")]
public class MoviesController : ControllerBase
{
    private readonly IMovieService _movieService;

    public MoviesController(IMovieService movieService)
    {
        _movieService = movieService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<MovieDto>>> GetAll([FromQuery] string? search)
    {
        return Ok(await _movieService.GetAllAsync(search));
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<MovieDto>> GetById(string id)
    {
        var movie = await _movieService.GetByIdAsync(id);
        return movie is null ? NotFound() : Ok(movie);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<MovieDto>> Create(UpsertMovieRequest request)
    {
        var created = await _movieService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<MovieDto>> Update(string id, UpsertMovieRequest request)
    {
        var updated = await _movieService.UpdateAsync(id, request);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await _movieService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
