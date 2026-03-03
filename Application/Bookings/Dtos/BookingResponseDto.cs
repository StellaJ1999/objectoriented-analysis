namespace Application.Bookings.Dtos;

public sealed record BookingResponseDto
{
    public Guid Id { get; init; }
    public Guid RoomId { get; init; }
    public string RoomName { get; init; } = string.Empty;
    public Guid UserId { get; init; }
    public string UserFullName { get; init; } = string.Empty;
    public DateTime StartTime { get; init; }
    public DateTime EndTime { get; init; }
    public string? Purpose { get; init; }
    public string Status { get; init; } = string.Empty;
}