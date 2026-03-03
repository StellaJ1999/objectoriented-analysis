using Domain.Common.Abstractions.Repositories;
using Domain.Rooms;
using Domain.Common.ValueObjects;
using Infrastructure.Persistense.EFCore.Context;
using Infrastructure.Persistense.EFCore.Entities.Factories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistense.EFCore.Repositories;

/// Repository för Room-aggregat med persistence factories
public class RoomRepository : RepositoryBase<Room, Guid>, IRoomRepository
{
    public RoomRepository(ApplicationDbContext context) : base(context) { }

    /// Hämtar lediga rum under ett tidsintervall
    public async Task<IEnumerable<Room>> GetAvailableRoomsAsync(
        TimeInterval timeInterval,
        CancellationToken cancellationToken = default)
    {
        if (timeInterval == null)
            throw new ArgumentNullException(nameof(timeInterval));

        // Hitta rum som INTE har överlappande aktiva bokningar
        var bookedRoomIds = await _context.Bookings
            .AsNoTracking()
            .Where(b =>
                b.Status == "Active" &&
                b.TimeInterval.StartTime < timeInterval.EndTime &&
                b.TimeInterval.EndTime > timeInterval.StartTime)
            .Select(b => b.RoomId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var roomsData = await _context.Rooms
            .AsNoTracking()
            .Where(r =>
                r.IsActive &&
                !bookedRoomIds.Contains(r.Id))
            .Select(r => new
            {
                r.Id,
                r.Name,
                r.Capacity,
                r.Location,
                r.IsActive
            })
            .ToListAsync(cancellationToken);

        return roomsData.Select(r => RoomFactory.Reconstitute(
            r.Id,
            r.Name,
            r.Capacity,
            r.Location,
            r.IsActive));
    }

    /// Hämtar rum baserat på namn
    public async Task<Room?> GetByNameAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty", nameof(name));

        var roomData = await _context.Rooms
            .AsNoTracking()
            .Where(r => r.Name == name)
            .Select(r => new
            {
                r.Id,
                r.Name,
                r.Capacity,
                r.Location,
                r.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (roomData == null)
            return null;

        return RoomFactory.Reconstitute(
            roomData.Id,
            roomData.Name,
            roomData.Capacity,
            roomData.Location,
            roomData.IsActive);
    }

    /// Override GetAllAsync för att använda factory
    public override async Task<IReadOnlyList<Room>> GetAllAsync(
        CancellationToken ct = default)
    {
        var roomsData = await _context.Rooms
            .AsNoTracking()
            .Where(r => r.IsActive)
            .Select(r => new
            {
                r.Id,
                r.Name,
                r.Capacity,
                r.Location,
                r.IsActive
            })
            .ToListAsync(ct);

        return roomsData
            .Select(r => RoomFactory.Reconstitute(
                r.Id,
                r.Name,
                r.Capacity,
                r.Location,
                r.IsActive))
            .ToList();
    }
}