using Application.Users.Abstractions;
using Application.Users.Dtos;
using Domain.Common.Abstractions.Repositories;
using Domain.Common.ValueObjects;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;

namespace Application.Users.Services;

public sealed class AuthenticationService : IAuthenticationService
{
    private readonly IUserRepository _userRepository;

    public AuthenticationService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginDto dto, CancellationToken ct = default)
    {
        // Hämta användare baserat på email
        var emailAddress = EmailAddress.Create(dto.Email.ToLowerInvariant());
        var user = await _userRepository.GetByEmailAsync(emailAddress, ct);

        if (user == null || !user.IsActive)
            return null;

        var token = GenerateToken(user.Id);
        var expiresAt = DateTime.UtcNow.AddHours(8);

        return new LoginResponseDto
        {
            UserId = user.Id,
            Email = user.Email.Value,
            FullName = user.FullName,
            Role = user.Role.Name,
            Token = token,
            ExpiresAt = expiresAt
        };
    }

    public Task<bool> ValidateTokenAsync(string token, CancellationToken ct = default)
    {
        return Task.FromResult(!string.IsNullOrEmpty(token));
    }

    public async Task<UserDto?> GetUserFromTokenAsync(string token, CancellationToken ct = default)
    {
        return await Task.FromResult<UserDto?>(null);
    }

    private static string GenerateToken(Guid userId)
    {
        var bytes = Encoding.UTF8.GetBytes($"{userId}:{DateTime.UtcNow.Ticks}");
        return Convert.ToBase64String(bytes);
    }

}