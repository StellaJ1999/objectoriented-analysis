namespace Application.Bookings.Dtos;

public sealed record CreateBookingDto
{
    public Guid RoomId { get; init; }
    public Guid? BookedForUserId { get; init; } // Null = för sig själv, annars för annan (receptionist)
    public DateTime StartTime { get; init; }
    public DateTime EndTime { get; init; }
    public string? Purpose { get; init; }
}