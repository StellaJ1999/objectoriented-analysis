namespace Application.Rooms.Dtos;

public sealed record RoomDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int Capacity { get; init; }
    public string? Location { get; init; }
    public bool IsActive { get; init; }
}