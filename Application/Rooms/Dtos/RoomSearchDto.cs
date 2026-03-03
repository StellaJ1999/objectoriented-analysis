namespace Application.Rooms.Dtos;

public sealed record RoomSearchDto
{
    public DateTime StartTime { get; init; }
    public DateTime EndTime { get; init; }
    public int? MinCapacity { get; init; } // UC-3: filtrera på kapacitet
    public bool OnlyActive { get; init; } = true;
}