using Domain.Bookings;
using Domain.Common.ValueObjects;
using System.Reflection;

namespace Infrastructure.Persistense.EFCore.Entities.Factories;

/// Factory för att rekonstituera Booking-aggregat från persistence layer.
/// Används för att återskapa entities från databas utan att köra business logic.
public static class BookingFactory
{

    public static Booking Reconstitute(
        Guid id,
        Guid roomId,
        Guid userId,
        DateTime startTime,
        DateTime endTime,
        string? purpose,
        string status)
    {
        // Skapa instans utan att anropa constructor (kringgår validering)
        var booking = CreateUninitializedBooking();

        // Skapa TimeInterval - vi behöver kringgå validering här också
        var timeInterval = CreateTimeInterval(startTime, endTime);

        // Sätt properties via reflection
        SetProperty(booking, nameof(Booking.Id), id);
        SetProperty(booking, nameof(Booking.RoomId), roomId);
        SetProperty(booking, nameof(Booking.UserId), userId);
        SetProperty(booking, nameof(Booking.TimeInterval), timeInterval);
        SetProperty(booking, nameof(Booking.Purpose), purpose);
        SetProperty(booking, nameof(Booking.Status), status);

        return booking;
    }

    /// Rekonstituerar från en tuple (användbart för EF Core projections)
    public static Booking Reconstitute(
        (Guid Id, Guid RoomId, Guid UserId, DateTime StartTime,
         DateTime EndTime, string? Purpose, string Status) data)
    {
        return Reconstitute(
            data.Id,
            data.RoomId,
            data.UserId,
            data.StartTime,
            data.EndTime,
            data.Purpose,
            data.Status);
    }

    /// Skapar en oinitialiserad Booking-instans utan att köra constructor
    private static Booking CreateUninitializedBooking()
    {
        return (Booking)System.Runtime.Serialization.FormatterServices
            .GetUninitializedObject(typeof(Booking));
    }

    /// Skapar TimeInterval utan att köra validering (för historisk data)
    private static TimeInterval CreateTimeInterval(DateTime startTime, DateTime endTime)
    {
        // För rekonstituering av gammal data behöver vi kringgå validering
        // eftersom data kan vara i det förflutna
        var timeInterval = (TimeInterval)System.Runtime.Serialization.FormatterServices
            .GetUninitializedObject(typeof(TimeInterval));

        var startProperty = typeof(TimeInterval).GetProperty(nameof(TimeInterval.StartTime));
        var endProperty = typeof(TimeInterval).GetProperty(nameof(TimeInterval.EndTime));

        startProperty?.SetValue(timeInterval, startTime);
        endProperty?.SetValue(timeInterval, endTime);

        return timeInterval;
    }

    /// Sätter en property via reflection
    private static void SetProperty<T>(Booking booking, string propertyName, T value)
    {
        var property = typeof(Booking).GetProperty(
            propertyName,
            BindingFlags.Public | BindingFlags.Instance);

        if (property == null)
            throw new InvalidOperationException(
                $"Property '{propertyName}' not found on Booking type");

        property.SetValue(booking, value);
    }
}