using Application.Rooms.Dtos;

namespace Application.Rooms.Abstractions;

public interface IRoomService
{
    // UC-8: Administrera rum - skapa
    Task<RoomDto> CreateRoomAsync(RoomDto dto, CancellationToken ct = default);

    // UC-8: Administrera rum - uppdatera
    Task<RoomDto> UpdateRoomAsync(Guid roomId, RoomDto dto, CancellationToken ct = default);

    // UC-8: Administrera rum - inaktivera
    Task<bool> DeactivateRoomAsync(Guid roomId, CancellationToken ct = default);

    // UC-8: Aktivera rum
    Task<bool> ActivateRoomAsync(Guid roomId, CancellationToken ct = default);

    // UC-2: Visa kalenderöversikt - alla rum
    Task<IEnumerable<RoomDto>> GetAllRoomsAsync(bool onlyActive = true, CancellationToken ct = default);

    // UC-3: Söka lediga rum
    Task<IEnumerable<RoomDto>> GetAvailableRoomsAsync(RoomSearchDto searchDto, CancellationToken ct = default);

    // Hämta specifikt rum
    Task<RoomDto?> GetRoomByIdAsync(Guid roomId, CancellationToken ct = default);

    // Kontrollera om rum är ledigt
    Task<bool> IsRoomAvailableAsync(Guid roomId, DateTime startTime, DateTime endTime, Guid? excludeBookingId = null, CancellationToken ct = default);
}