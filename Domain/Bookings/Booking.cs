namespace Domain.Bookings;
using System.ComponentModel.DataAnnotations;
using Domain.Common;
using Domain.Common.ValueObjects;

/// Aggregate Root
public sealed class Booking
{
    public Guid Id { get; private set; }
    public Guid RoomId { get; private set; }
    public Guid UserId { get; private set; }
    public TimeInterval TimeInterval { get; private set; }
    public string? Purpose { get; private set; }
    public string Status { get; private set; }

    // EF Core behöver en tom constructor
    private Booking() { }

    public Booking(Guid roomId, Guid userId, TimeInterval timeInterval, string? purpose = null)
    {
        if (roomId == Guid.Empty)
            throw new ValidationException("RoomId måste anges");

        if (userId == Guid.Empty)
            throw new ValidationException("UserId måste anges");

        Id = Guid.NewGuid();
        RoomId = roomId;
        UserId = userId;
        TimeInterval = timeInterval ?? throw new ArgumentNullException(nameof(timeInterval));
        Purpose = purpose;
        Status = "Active";
    }

    public void Cancel()
    {
        if (Status != "Active")
            throw new ValidationException("Endast aktiva bokningar kan avbokas");

        Status = "Cancelled";
    }

    public void Reschedule(TimeInterval newTimeInterval)
    {
        if (Status != "Active")
            throw new ValidationException("Endast aktiva bokningar kan ändras");

        TimeInterval = newTimeInterval ?? throw new ArgumentNullException(nameof(newTimeInterval));
    }

    public bool IsOwnedBy(Guid userId) => UserId == userId;

    public bool OverlapsWith(TimeInterval interval) =>
        Status == "Active" && TimeInterval.OverlapsWith(interval);
}
