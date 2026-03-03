using Application.Rooms.Abstractions;
using Application.Rooms.Dtos;
using Domain.Common.Abstractions.Repositories;
using Domain.Common.ValueObjects;
using Domain.Rooms;

namespace Application.Rooms.Services;

public sealed class RoomService : IRoomService
{
    private readonly IRoomRepository _roomRepository;
    private readonly IBookingRepository _bookingRepository;

    public RoomService(IRoomRepository roomRepository, IBookingRepository bookingRepository)
    {
        _roomRepository = roomRepository;
        _bookingRepository = bookingRepository;
    }

    public async Task<RoomDto> CreateRoomAsync(RoomDto dto, CancellationToken ct = default)
    {
        // Validera att rummet inte redan finns
        var existingRoom = await _roomRepository.GetByNameAsync(dto.Name, ct);
        if (existingRoom != null)
            throw new InvalidOperationException($"Ett rum med namnet '{dto.Name}' finns redan");

        // Skapa rum
        var room = new Room(dto.Name, dto.Capacity, dto.Location);

        await _roomRepository.CreateAsync(room, ct);

        return MapToDto(room);
    }

    public async Task<RoomDto> UpdateRoomAsync(Guid roomId, RoomDto dto, CancellationToken ct = default)
    {
        var room = await _roomRepository.GetByIdAsync(roomId, ct);
        if (room == null)
            throw new InvalidOperationException("Rummet finns inte");

        // Uppdatera detaljer
        room.UpdateDetails(dto.Name, dto.Capacity, dto.Location);

        await _roomRepository.UpdateAsync(roomId, room, ct);

        return MapToDto(room);
    }

    public async Task<bool> DeactivateRoomAsync(Guid roomId, CancellationToken ct = default)
    {
        var room = await _roomRepository.GetByIdAsync(roomId, ct);
        if (room == null)
            throw new InvalidOperationException("Rummet finns inte");

        // Kontrollera om det finns framtida bokningar
        var futureBookings = await _bookingRepository.GetByRoomIdAsync(roomId, ct);
        var hasActiveBookings = futureBookings.Any(b =>
            b.Status == "Active" &&
            b.TimeInterval.StartTime > DateTime.UtcNow);

        if (hasActiveBookings)
            throw new InvalidOperationException("Rummet har aktiva bokningar och kan inte inaktiveras");

        room.Deactivate();

        return await _roomRepository.UpdateAsync(roomId, room, ct);
    }

    public async Task<bool> ActivateRoomAsync(Guid roomId, CancellationToken ct = default)
    {
        var room = await _roomRepository.GetByIdAsync(roomId, ct);
        if (room == null)
            throw new InvalidOperationException("Rummet finns inte");

        room.Activate();

        return await _roomRepository.UpdateAsync(roomId, room, ct);
    }

    public async Task<IEnumerable<RoomDto>> GetAllRoomsAsync(bool onlyActive = true, CancellationToken ct = default)
    {
        var rooms = await _roomRepository.GetAllAsync(ct);

        if (onlyActive)
            rooms = rooms.Where(r => r.IsActive).ToList();

        return rooms.Select(MapToDto);
    }

    public async Task<IEnumerable<RoomDto>> GetAvailableRoomsAsync(RoomSearchDto searchDto, CancellationToken ct = default)
    {
        // Hämta alla aktiva rum
        var rooms = await _roomRepository.GetAllAsync(ct);
        var activeRooms = rooms.Where(r => searchDto.OnlyActive ? r.IsActive : true);

        // Filtrera på kapacitet
        if (searchDto.MinCapacity.HasValue)
            activeRooms = activeRooms.Where(r => r.Capacity >= searchDto.MinCapacity.Value);

        // Skapa TimeInterval för sökning
        var timeInterval = new TimeInterval(searchDto.StartTime, searchDto.EndTime);

        // Filtrera ut rum som har överlappande bokningar
        var availableRooms = new List<Room>();
        foreach (var room in activeRooms)
        {
            var conflictingBookings = await _bookingRepository.GetConflictingBookingsAsync(
                room.Id,
                timeInterval,
                ct);

            if (!conflictingBookings.Any())
                availableRooms.Add(room);
        }

        return availableRooms.Select(MapToDto);
    }

    public async Task<RoomDto?> GetRoomByIdAsync(Guid roomId, CancellationToken ct = default)
    {
        var room = await _roomRepository.GetByIdAsync(roomId, ct);
        return room == null ? null : MapToDto(room);
    }

    public async Task<bool> IsRoomAvailableAsync(
        Guid roomId,
        DateTime startTime,
        DateTime endTime,
        Guid? excludeBookingId = null,
        CancellationToken ct = default)
    {
        var timeInterval = new TimeInterval(startTime, endTime);
        var conflictingBookings = await _bookingRepository.GetConflictingBookingsAsync(
            roomId,
            timeInterval,
            ct);

        if (excludeBookingId.HasValue)
            conflictingBookings = conflictingBookings.Where(b => b.Id != excludeBookingId.Value);

        return !conflictingBookings.Any();
    }

    private static RoomDto MapToDto(Room room)
    {
        return new RoomDto
        {
            Id = room.Id,
            Name = room.Name,
            Capacity = room.Capacity,
            Location = room.Location,
            IsActive = room.IsActive
        };
    }
}