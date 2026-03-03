namespace Application.Bookings.Dtos;

public sealed record UpdateBookingDto
{
    public DateTime StartTime { get; init; }
    public DateTime EndTime { get; init; }
    public string? Purpose { get; init; }
}