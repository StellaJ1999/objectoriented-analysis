using Domain.Rooms;
using System.Reflection;

namespace Infrastructure.Persistense.EFCore.Entities.Factories;

/// Factory för att rekonstituera Room-aggregat från persistence layer.
/// Används för att återskapa entities från databas utan att köra business logic.
public static class RoomFactory
{

    public static Room Reconstitute(
        Guid id,
        string name,
        int capacity,
        string? location,
        bool isActive)
    {
        // Skapa instans utan att anropa constructor (kringgår validering)
        var room = CreateUninitializedRoom();

        // Sätt properties via reflection
        SetProperty(room, nameof(Room.Id), id);
        SetProperty(room, nameof(Room.Name), name);
        SetProperty(room, nameof(Room.Capacity), capacity);
        SetProperty(room, nameof(Room.Location), location);
        SetProperty(room, nameof(Room.IsActive), isActive);

        return room;
    }

    /// Rekonstituerar från en tuple (användbart för EF Core projections)
    public static Room Reconstitute(
        (Guid Id, string Name, int Capacity, string? Location, bool IsActive) data)
    {
        return Reconstitute(
            data.Id,
            data.Name,
            data.Capacity,
            data.Location,
            data.IsActive);
    }

    /// Skapar en oinitialiserad Room-instans utan att köra constructor
    private static Room CreateUninitializedRoom()
    {
        return (Room)System.Runtime.Serialization.FormatterServices
            .GetUninitializedObject(typeof(Room));
    }

    /// Sätter en property via reflection
    private static void SetProperty<T>(Room room, string propertyName, T value)
    {
        var property = typeof(Room).GetProperty(
            propertyName,
            BindingFlags.Public | BindingFlags.Instance);

        if (property == null)
            throw new InvalidOperationException(
                $"Property '{propertyName}' not found on Room type");

        property.SetValue(room, value);
    }
}