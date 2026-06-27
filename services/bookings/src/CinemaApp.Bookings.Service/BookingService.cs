using CinemaApp.Bookings.Domain.Dto;
using CinemaApp.Bookings.Domain.Events;
using CinemaApp.Bookings.Domain.Models;
using CinemaApp.Bookings.Repository;
using Microsoft.Extensions.Configuration;

namespace CinemaApp.Bookings.Service;

public enum LockSeatResult
{
    Locked,
    AlreadyLocked,
    SeatNotAvailable,
    ScreeningNotFound
}

public enum BookingCreationResult
{
    Success,
    SeatNotLocked,
    SeatNoLongerAvailable,
    ScreeningNotFound
}

public interface IBookingService
{
    Task<IEnumerable<ScreeningDto>> GetScreeningsAsync(string? movieId = null);
    Task<SeatMapDto?> GetSeatMapAsync(string screeningId, string? currentUserId);
    Task<LockSeatResult> LockSeatAsync(string screeningId, string seatId, string userId);
    Task<bool> ReleaseSeatAsync(string screeningId, string seatId, string userId);
    Task<(BookingCreationResult Result, BookingDto? Booking)> CreateBookingAsync(string screeningId, List<string> seatIds, string userId, string userEmail);
    Task<IEnumerable<BookingDto>> GetMyBookingsAsync(string userId);
}

public class BookingService : IBookingService
{
    private readonly IScreeningRepository _screeningRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly ISeatLockService _seatLockService;
    private readonly ISeatStatusNotifier _notifier;
    private readonly IEventPublisher _eventPublisher;
    private readonly TimeSpan _lockTtl;

    public BookingService(
        IScreeningRepository screeningRepository,
        IBookingRepository bookingRepository,
        ISeatLockService seatLockService,
        ISeatStatusNotifier notifier,
        IEventPublisher eventPublisher,
        IConfiguration configuration)
    {
        _screeningRepository = screeningRepository;
        _bookingRepository = bookingRepository;
        _seatLockService = seatLockService;
        _notifier = notifier;
        _eventPublisher = eventPublisher;

        var ttlSeconds = configuration.GetValue<int?>("SeatLock:TtlSeconds") ?? 300;
        _lockTtl = TimeSpan.FromSeconds(ttlSeconds);
    }

    public async Task<IEnumerable<ScreeningDto>> GetScreeningsAsync(string? movieId = null)
    {
        var screenings = movieId is null
            ? await _screeningRepository.GetAllAsync()
            : await _screeningRepository.GetAllAsync(s => s.MovieId == movieId);

        return screenings.Select(s => new ScreeningDto(
            s.Id,
            s.MovieId,
            s.HallId,
            s.StartTime,
            s.BasePrice,
            s.Seats.Count,
            s.Seats.Count(seat => seat.Status == SeatStatus.Available)));
    }

    public async Task<SeatMapDto?> GetSeatMapAsync(string screeningId, string? currentUserId)
    {
        var screening = await _screeningRepository.GetByIdAsync(screeningId);
        if (screening is null)
        {
            return null;
        }

        var seatDtos = new List<SeatDto>();
        foreach (var seat in screening.Seats)
        {
            if (seat.Status == SeatStatus.Booked)
            {
                seatDtos.Add(new SeatDto(seat.SeatId, seat.Row, seat.Number, "Booked", false));
                continue;
            }

            var lockOwner = await _seatLockService.GetLockOwnerAsync(screeningId, seat.SeatId);
            if (lockOwner is null)
            {
                seatDtos.Add(new SeatDto(seat.SeatId, seat.Row, seat.Number, "Available", false));
            }
            else
            {
                seatDtos.Add(new SeatDto(seat.SeatId, seat.Row, seat.Number, "Locked", lockOwner == currentUserId));
            }
        }

        return new SeatMapDto(screeningId, seatDtos);
    }

    public async Task<LockSeatResult> LockSeatAsync(string screeningId, string seatId, string userId)
    {
        var screening = await _screeningRepository.GetByIdAsync(screeningId);
        if (screening is null)
        {
            return LockSeatResult.ScreeningNotFound;
        }

        var seat = screening.Seats.FirstOrDefault(s => s.SeatId == seatId);
        if (seat is null || seat.Status == SeatStatus.Booked)
        {
            return LockSeatResult.SeatNotAvailable;
        }

        var acquired = await _seatLockService.TryLockSeatAsync(screeningId, seatId, userId, _lockTtl);
        if (!acquired)
        {
            return LockSeatResult.AlreadyLocked;
        }

        await _notifier.NotifySeatStatusChangedAsync(screeningId, seatId, "Locked");
        return LockSeatResult.Locked;
    }

    public async Task<bool> ReleaseSeatAsync(string screeningId, string seatId, string userId)
    {
        var released = await _seatLockService.ReleaseLockAsync(screeningId, seatId, userId);
        if (released)
        {
            await _notifier.NotifySeatStatusChangedAsync(screeningId, seatId, "Available");
        }

        return released;
    }

    public async Task<(BookingCreationResult Result, BookingDto? Booking)> CreateBookingAsync(
        string screeningId, List<string> seatIds, string userId, string userEmail)
    {
        var screening = await _screeningRepository.GetByIdAsync(screeningId);
        if (screening is null)
        {
            return (BookingCreationResult.ScreeningNotFound, null);
        }

        // Re-validate everything at confirmation time - never trust client state.
        foreach (var seatId in seatIds)
        {
            var seat = screening.Seats.FirstOrDefault(s => s.SeatId == seatId);
            if (seat is null || seat.Status == SeatStatus.Booked)
            {
                return (BookingCreationResult.SeatNoLongerAvailable, null);
            }

            var lockOwner = await _seatLockService.GetLockOwnerAsync(screeningId, seatId);
            if (lockOwner != userId)
            {
                return (BookingCreationResult.SeatNotLocked, null);
            }
        }

        foreach (var seatId in seatIds)
        {
            var seat = screening.Seats.First(s => s.SeatId == seatId);
            seat.Status = SeatStatus.Booked;
        }

        await _screeningRepository.ReplaceAsync(screening);

        var booking = new Booking
        {
            UserId = userId,
            UserEmail = userEmail,
            ScreeningId = screeningId,
            SeatIds = seatIds,
            TotalPrice = screening.BasePrice * seatIds.Count,
            Status = BookingStatus.Confirmed
        };

        await _bookingRepository.InsertAsync(booking);
        await _seatLockService.ReleaseLocksAsync(screeningId, seatIds);

        foreach (var seatId in seatIds)
        {
            await _notifier.NotifySeatStatusChangedAsync(screeningId, seatId, "Booked");
        }

        _eventPublisher.Publish("booking.confirmed", new BookingConfirmedEvent(
            booking.Id, userId, userEmail, screeningId, seatIds, booking.TotalPrice, DateTime.UtcNow));

        return (BookingCreationResult.Success, ToDto(booking));
    }

    public async Task<IEnumerable<BookingDto>> GetMyBookingsAsync(string userId)
    {
        var bookings = await _bookingRepository.GetAllAsync(b => b.UserId == userId);
        return bookings.Select(ToDto);
    }

    private static BookingDto ToDto(Booking booking) => new(
        booking.Id, booking.ScreeningId, booking.SeatIds, booking.TotalPrice, booking.Status.ToString(), booking.CreatedAt);
}
