namespace Domain.Users;

using Domain.Common;
using Domain.Common.ValueObjects;
using System.ComponentModel.DataAnnotations;

public sealed class User
{
    public Guid Id { get; private set; }
    public EmailAddress Email { get; private set; }
    public string FullName { get; private set; }
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; }

    // EF Core behöver en tom constructor
    private User() { }

    public User(string email, string fullName, UserRole? role = null)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ValidationException("Email måste anges");

        if (string.IsNullOrWhiteSpace(fullName))
            throw new ValidationException("Namn måste anges");

        Id = Guid.NewGuid();
        Email = EmailAddress.Create(email.ToLowerInvariant());
        FullName = fullName;
        Role = role ?? UserRole.Employee;
        IsActive = true;
    }

    public void UpdateRole(UserRole newRole) => Role = newRole;

    public bool IsReceptionist() => Role == UserRole.Receptionist;

    public bool CanBookForOthers() => Role == UserRole.Receptionist;

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
