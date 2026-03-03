using Domain.Users;
using Domain.Common.ValueObjects;
using System.Reflection;

namespace Infrastructure.Persistense.EFCore.Entities.Factories;

/// Factory för att rekonstituera User-aggregat från persistence layer.
/// Används för att återskapa entities från databas utan att köra business logic.
public static class UserFactory
{
   
    public static User Reconstitute(
        Guid id,
        string email,
        string fullName,
        string role,
        bool isActive)
    {
        // Skapa instans utan att anropa constructor (kringgår validering)
        var user = CreateUninitializedUser();

        // Konvertera role string till UserRole
        var userRole = role == "Employee" ? UserRole.Employee : UserRole.Receptionist;

        // Sätt properties via reflection
        SetProperty(user, nameof(User.Id), id);
        SetProperty(user, nameof(User.Email), EmailAddress.Create(email));
        SetProperty(user, nameof(User.FullName), fullName);
        SetProperty(user, nameof(User.Role), userRole);
        SetProperty(user, nameof(User.IsActive), isActive);

        return user;
    }

    /// Rekonstituerar från en tuple (användbart för EF Core projections)
    public static User Reconstitute(
        (Guid Id, string Email, string FullName, string Role, bool IsActive) data)
    {
        return Reconstitute(
            data.Id,
            data.Email,
            data.FullName,
            data.Role,
            data.IsActive);
    }

    /// Skapar en oinitialiserad User-instans utan att köra constructor
    private static User CreateUninitializedUser()
    {
        return (User)System.Runtime.Serialization.FormatterServices
            .GetUninitializedObject(typeof(User));
    }

    /// Sätter en property via reflection
    private static void SetProperty<T>(User user, string propertyName, T value)
    {
        var property = typeof(User).GetProperty(
            propertyName,
            BindingFlags.Public | BindingFlags.Instance);

        if (property == null)
            throw new InvalidOperationException(
                $"Property '{propertyName}' not found on User type");

        property.SetValue(user, value);
    }
}