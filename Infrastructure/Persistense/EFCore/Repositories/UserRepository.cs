using Domain.Common.Abstractions.Repositories;
using Domain.Users;
using Domain.Common.ValueObjects;
using Infrastructure.Persistense.EFCore.Context;
using Infrastructure.Persistense.EFCore.Entities.Factories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistense.EFCore.Repositories;

/// Repository för User-aggregat med persistence factories
public class UserRepository : RepositoryBase<User, Guid>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context) { }

    /// Hämtar användare baserat på email
    public async Task<User?> GetByEmailAsync(
        EmailAddress email,
        CancellationToken cancellationToken = default)
    {
        if (email == null)
            throw new ArgumentNullException(nameof(email));

        // Läs rådata från databas med projection för prestanda
        var userData = await _context.Users
            .AsNoTracking()
            .Where(u => u.Email == email)
            .Select(u => new
            {
                u.Id,
                Email = u.Email.Value,
                u.FullName,
                Role = u.Role.Name,
                u.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (userData == null)
            return null;

        // Använd factory för att rekonstituera
        return UserFactory.Reconstitute(
            userData.Id,
            userData.Email,
            userData.FullName,
            userData.Role,
            userData.IsActive);
    }

    /// Hämtar användare baserat på roll
    public async Task<IEnumerable<User>> GetByRoleAsync(
        UserRole role,
        CancellationToken cancellationToken = default)
    {
        if (role == null)
            throw new ArgumentNullException(nameof(role));

        var usersData = await _context.Users
            .AsNoTracking()
            .Where(u => u.Role == role)
            .Select(u => new
            {
                u.Id,
                Email = u.Email.Value,
                u.FullName,
                Role = u.Role.Name,
                u.IsActive
            })
            .ToListAsync(cancellationToken);

        return usersData.Select(u => UserFactory.Reconstitute(
            u.Id,
            u.Email,
            u.FullName,
            u.Role,
            u.IsActive));
    }

    /// Kollar om en email redan finns i systemet
    public async Task<bool> EmailExistsAsync(
        EmailAddress email,
        CancellationToken cancellationToken = default)
    {
        if (email == null)
            throw new ArgumentNullException(nameof(email));

        return await _context.Users
            .AsNoTracking()
            .AnyAsync(u => u.Email == email, cancellationToken);
    }

    /// Override GetAllAsync för att använda factory
    public override async Task<IReadOnlyList<User>> GetAllAsync(
        CancellationToken ct = default)
    {
        var usersData = await _context.Users
            .AsNoTracking()
            .Select(u => new
            {
                u.Id,
                Email = u.Email.Value,
                u.FullName,
                Role = u.Role.Name,
                u.IsActive
            })
            .ToListAsync(ct);

        return usersData
            .Select(u => UserFactory.Reconstitute(
                u.Id,
                u.Email,
                u.FullName,
                u.Role,
                u.IsActive))
            .ToList();
    }
}