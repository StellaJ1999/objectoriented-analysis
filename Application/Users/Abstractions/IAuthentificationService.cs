using Application.Users.Dtos;

namespace Application.Users.Abstractions;

public interface IAuthenticationService
{
    // UC-1: Logga in
    Task<LoginResponseDto?> LoginAsync(LoginDto dto, CancellationToken ct = default);

    // Validera token
    Task<bool> ValidateTokenAsync(string token, CancellationToken ct = default);

    // Hämta användare från token
    Task<UserDto?> GetUserFromTokenAsync(string token, CancellationToken ct = default);
}