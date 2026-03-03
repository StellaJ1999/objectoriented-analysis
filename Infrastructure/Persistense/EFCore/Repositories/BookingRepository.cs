using Domain.Common.Abstractions.Repositories;
using Domain.Bookings;
using Domain.Common.ValueObjects;
using Infrastructure.Persistense.EFCore.Context;
using Infrastructure.Persistense.EFCore.Entities.Factories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistense.EFCore.Repositories;

/// Repository för Booking-aggregat med persistence factories
public class BookingRepository : RepositoryBase<Booking, Guid>, IBookingRepository
{
    public BookingRepository(ApplicationDbContext context) : base(context) { }

    /// Hämtar bokningar för en specifik användare
    public async Task<IEnumerable<Booking>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var bookingsData = await _context.Bookings
            .AsNoTracking()
            .Where(b => b.UserId == userId)
            .Select(b => new
            {
                b.Id,
                b.RoomId,
                b.UserId,
                StartTime = b.TimeInterval.StartTime,
                EndTime = b.TimeInterval.EndTime,
                b.Purpose,
                b.Status
            })
            .ToListAsync(cancellationToken);

        return bookingsData.Select(b => BookingFactory.Reconstitute(
            b.Id,
            b.RoomId,
            b.UserId,
            b.StartTime,
            b.EndTime,
            b.Purpose,
            b.Status));
    }

    /// Hämtar bokningar för ett specifikt rum
    public async Task<IEnumerable<Booking>> GetByRoomIdAsync(
        Guid roomId,
        CancellationToken cancellationToken = default)
    {
        var bookingsData = await _context.Bookings
            .AsNoTracking()
            .Where(b => b.RoomId == roomId && b.Status == "Active")
            .Select(b => new
            {
                b.Id,
                b.RoomId,
                b.UserId,
                StartTime = b.TimeInterval.StartTime,
                EndTime = b.TimeInterval.EndTime,
                b.Purpose,
                b.Status
            })
            .ToListAsync(cancellationToken);

        return bookingsData.Select(b => BookingFactory.Reconstitute(
            b.Id,
            b.RoomId,
            b.UserId,
            b.StartTime,
            b.EndTime,
            b.Purpose,
            b.Status));
    }

    /// Hämtar bokningar som överlappar med ett tidsinterval
    public async Task<IEnumerable<Booking>> GetConflictingBookingsAsync(
        Guid roomId,
        TimeInterval timeInterval,
        CancellationToken cancellationToken = default)
    {
        if (timeInterval == null)
            throw new ArgumentNullException(nameof(timeInterval));

        var bookingsData = await _context.Bookings
            .AsNoTracking()
            .Where(b =>
                b.RoomId == roomId &&
                b.Status == "Active" &&
                b.TimeInterval.StartTime < timeInterval.EndTime &&
                b.TimeInterval.EndTime > timeInterval.StartTime)
            .Select(b => new
            {
                b.Id,
                b.RoomId,
                b.UserId,
                StartTime = b.TimeInterval.StartTime,
                EndTime = b.TimeInterval.EndTime,
                b.Purpose,
                b.Status
            })
            .ToListAsync(cancellationToken);

        return bookingsData.Select(b => BookingFactory.Reconstitute(
            b.Id,
            b.RoomId,
            b.UserId,
            b.StartTime,
            b.EndTime,
            b.Purpose,
            b.Status));
    }

    /// Hämtar kommande bokningar från ett visst datum
    public async Task<IEnumerable<Booking>> GetUpcomingBookingsAsync(
        DateTime from,
        CancellationToken cancellationToken = default)
    {
        var bookingsData = await _context.Bookings
            .AsNoTracking()
            .Where(b =>
                b.Status == "Active" &&
                b.TimeInterval.StartTime >= from)
            .OrderBy(b => b.TimeInterval.StartTime)
            .Select(b => new
            {
                b.Id,
                b.RoomId,
                b.UserId,
                StartTime = b.TimeInterval.StartTime,
                EndTime = b.TimeInterval.EndTime,
                b.Purpose,
                b.Status
            })
            .ToListAsync(cancellationToken);

        return bookingsData.Select(b => BookingFactory.Reconstitute(
            b.Id,
            b.RoomId,
            b.UserId,
            b.StartTime,
            b.EndTime,
            b.Purpose,
            b.Status));
    }

    /// Override GetAllAsync för att använda factory
    public override async Task<IReadOnlyList<Booking>> GetAllAsync(
        CancellationToken ct = default)
    {
        var bookingsData = await _context.Bookings
            .AsNoTracking()
            .Select(b => new
            {
                b.Id,
                b.RoomId,
                b.UserId,
                StartTime = b.TimeInterval.StartTime,
                EndTime = b.TimeInterval.EndTime,
                b.Purpose,
                b.Status
            })
            .ToListAsync(ct);

        return bookingsData
            .Select(b => BookingFactory.Reconstitute(
                b.Id,
                b.RoomId,
                b.UserId,
                b.StartTime,
                b.EndTime,
                b.Purpose,
                b.Status))
            .ToList();
    }
}