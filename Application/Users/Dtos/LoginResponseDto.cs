namespace Application.Users.Dtos;

public sealed record LoginResponseDto
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public string Token { get; init; } = string.Empty; // JWT eller session token
    public DateTime ExpiresAt { get; init; }
}