using System.Security.Claims;
using CinemaApp.Bookings.Domain.Dto;
using CinemaApp.Bookings.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaApp.Bookings.Api.Controllers;

[ApiController]
[Route("api/screenings")]
public class ScreeningsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public ScreeningsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<ScreeningDto>>> GetAll([FromQuery] string? movieId)
        => Ok(await _bookingService.GetScreeningsAsync(movieId));

    [HttpGet("{id}/seats")]
    [AllowAnonymous]
    public async Task<ActionResult<SeatMapDto>> GetSeatMap(string id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        var map = await _bookingService.GetSeatMapAsync(id, userId);
        return map is null ? NotFound() : Ok(map);
    }
}
