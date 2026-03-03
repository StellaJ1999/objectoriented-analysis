using Domain.Common.Abstractions.Base;
using Domain.Common.ValueObjects;
using Domain.Users;

namespace Domain.Common.Abstractions.Repositories;

public interface IUserRepository : IRepositoryBase<User, Guid>
{
    Task<User?> GetByEmailAsync(EmailAddress email, CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetByRoleAsync(UserRole role, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(EmailAddress email, CancellationToken cancellationToken = default);
}