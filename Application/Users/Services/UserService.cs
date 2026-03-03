using Application.Users.Abstractions;
using Application.Users.Dtos;
using Domain.Common.Abstractions.Repositories;
using Domain.Common.ValueObjects;
using Domain.Users;
using System.Net.Mail;

namespace Application.Users.Services;

public sealed class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto> CreateUserAsync(UserDto dto, CancellationToken ct = default)
    {
        // Validera att emailen inte redan finns
        var emailExists = await _userRepository.EmailExistsAsync(
            EmailAddress.Create(dto.Email),
            ct);

        if (emailExists)
            throw new InvalidOperationException($"En användare med email '{dto.Email}' finns redan");

        // Konvertera role string till UserRole
        var role = dto.Role.ToLower() switch
        {
            "receptionist" => UserRole.Receptionist,
            "employee" => UserRole.Employee,
            _ => UserRole.Employee
        };

        // Skapa användare
        var user = new User(dto.Email, dto.FullName, role);

        await _userRepository.CreateAsync(user, ct);

        return MapToDto(user);
    }

    public async Task<UserDto> UpdateUserAsync(Guid userId, UserDto dto, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, ct);
        if (user == null)
            throw new InvalidOperationException("Användaren finns inte");

        // Skapa ny användare med uppdaterad data (immutability pattern)
        // Alternativt: lägg till Update-metoder på User entity

        // För nu: bara uppdatera roll
        var role = dto.Role.ToLower() switch
        {
            "receptionist" => UserRole.Receptionist,
            "employee" => UserRole.Employee,
            _ => user.Role
        };

        if (role != user.Role)
            user.UpdateRole(role);

        await _userRepository.UpdateAsync(userId, user, ct);

        return MapToDto(user);
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, ct);
        return user == null ? null : MapToDto(user);
    }

    public async Task<UserDto?> GetUserByEmailAsync(string email, CancellationToken ct = default)
    {
        var emailAddress = EmailAddress.Create(email);
        var user = await _userRepository.GetByEmailAsync(emailAddress, ct);
        return user == null ? null : MapToDto(user);
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync(CancellationToken ct = default)
    {
        var users = await _userRepository.GetAllAsync(ct);
        return users.Select(MapToDto);
    }

    public async Task<IEnumerable<UserDto>> GetUsersByRoleAsync(string role, CancellationToken ct = default)
    {
        var userRole = role.ToLower() switch
        {
            "receptionist" => UserRole.Receptionist,
            "employee" => UserRole.Employee,
            _ => throw new ArgumentException($"Invalid role: {role}")
        };

        var users = await _userRepository.GetByRoleAsync(userRole, ct);
        return users.Select(MapToDto);
    }

    public async Task<bool> UpdateUserRoleAsync(Guid userId, string newRole, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, ct);
        if (user == null)
            throw new InvalidOperationException("Användaren finns inte");

        var role = newRole.ToLower() switch
        {
            "receptionist" => UserRole.Receptionist,
            "employee" => UserRole.Employee,
            _ => throw new ArgumentException($"Invalid role: {newRole}")
        };

        user.UpdateRole(role);

        return await _userRepository.UpdateAsync(userId, user, ct);
    }

    public async Task<bool> DeactivateUserAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, ct);
        if (user == null)
            throw new InvalidOperationException("Användaren finns inte");

        user.Deactivate();

        return await _userRepository.UpdateAsync(userId, user, ct);
    }

    public async Task<bool> ActivateUserAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, ct);
        if (user == null)
            throw new InvalidOperationException("Användaren finns inte");

        user.Activate();

        return await _userRepository.UpdateAsync(userId, user, ct);
    }

    private static UserDto MapToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email.Value,
            FullName = user.FullName,
            Role = user.Role.Name,
            IsActive = user.IsActive
        };
    }
}