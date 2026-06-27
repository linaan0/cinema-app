using System.Security.Claims;
using CinemaApp.Bookings.Domain.Dto;
using CinemaApp.Bookings.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaApp.Bookings.Api.Controllers;

[ApiController]
[Route("api/bookings")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    // Step 1: user clicks a seat -> try to acquire a short-lived Redis lock.
    [HttpPost("screenings/{screeningId}/seats/{seatId}/lock")]
    public async Task<IActionResult> LockSeat(string screeningId, string seatId)
    {
        var userId = GetUserId();
        var result = await _bookingService.LockSeatAsync(screeningId, seatId, userId);

        return result switch
        {
            LockSeatResult.Locked => Ok(new { status = "Locked" }),
            LockSeatResult.AlreadyLocked => Conflict(new { message = "This seat was just taken by someone else. Please pick another seat." }),
            LockSeatResult.SeatNotAvailable => Conflict(new { message = "This seat is already booked." }),
            LockSeatResult.ScreeningNotFound => NotFound(),
            _ => BadRequest()
        };
    }

    // Step 2 (optional): user changes their mind before paying -> release the lock early.
    [HttpPost("screenings/{screeningId}/seats/{seatId}/release")]
    public async Task<IActionResult> ReleaseSeat(string screeningId, string seatId)
    {
        var userId = GetUserId();
        var released = await _bookingService.ReleaseSeatAsync(screeningId, seatId, userId);
        return released ? Ok() : NotFound();
    }

    // Step 3: confirm the booking. Re-validates every seat lock + availability
    // before committing - this is what actually prevents double-booking.
    [HttpPost]
    public async Task<IActionResult> CreateBooking(CreateBookingRequest request)
    {
        var userId = GetUserId();
        var email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;

        var (result, booking) = await _bookingService.CreateBookingAsync(request.ScreeningId, request.SeatIds, userId, email);

        return result switch
        {
            BookingCreationResult.Success => Ok(booking),
            BookingCreationResult.SeatNotLocked => Conflict(new { message = "One or more seats are not locked by you. Lock the seats first, then confirm within the time limit." }),
            BookingCreationResult.SeatNoLongerAvailable => Conflict(new { message = "One or more seats have already been booked by someone else." }),
            BookingCreationResult.ScreeningNotFound => NotFound(),
            _ => BadRequest()
        };
    }

    [HttpGet("my")]
    public async Task<ActionResult<IEnumerable<BookingDto>>> GetMyBookings()
    {
        var userId = GetUserId();
        return Ok(await _bookingService.GetMyBookingsAsync(userId));
    }

    private string GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")
        ?? throw new UnauthorizedAccessException();
}
