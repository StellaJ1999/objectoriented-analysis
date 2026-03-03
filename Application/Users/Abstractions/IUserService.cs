using Application.Users.Dtos;

namespace Application.Users.Abstractions;

public interface IUserService
{
    // Skapa användare
    Task<UserDto> CreateUserAsync(UserDto dto, CancellationToken ct = default);

    // Uppdatera användare
    Task<UserDto> UpdateUserAsync(Guid userId, UserDto dto, CancellationToken ct = default);

    // Hämta användare
    Task<UserDto?> GetUserByIdAsync(Guid userId, CancellationToken ct = default);
    Task<UserDto?> GetUserByEmailAsync(string email, CancellationToken ct = default);

    // Lista användare
    Task<IEnumerable<UserDto>> GetAllUsersAsync(CancellationToken ct = default);
    Task<IEnumerable<UserDto>> GetUsersByRoleAsync(string role, CancellationToken ct = default);

    // Ändra roll (admin)
    Task<bool> UpdateUserRoleAsync(Guid userId, string newRole, CancellationToken ct = default);

    // Aktivera/inaktivera
    Task<bool> DeactivateUserAsync(Guid userId, CancellationToken ct = default);
    Task<bool> ActivateUserAsync(Guid userId, CancellationToken ct = default);
}