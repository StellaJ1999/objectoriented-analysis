namespace Domain.Common.ValueObjects;

using System.ComponentModel.DataAnnotations;

using Domain.Common;

public record TimeInterval
{
    public DateTime StartTime { get; init; }
    public DateTime EndTime { get; init; }

    public TimeInterval(DateTime startTime, DateTime endTime)
    {
        if (endTime <= startTime)
            throw new ValidationException("Sluttid måste vara efter starttid");

        if (startTime < DateTime.UtcNow)
            throw new ValidationException("Starttid kan inte vara i det förflutna");

        StartTime = startTime;
        EndTime = endTime;
    }


    public bool OverlapsWith(TimeInterval other)
    {
        return StartTime < other.EndTime && EndTime > other.StartTime;
    }

    public TimeSpan Duration => EndTime - StartTime;
}