using Domain.Common.Abstractions.Base;
using Domain.Common.ValueObjects;
using Domain.Rooms;

namespace Domain.Common.Abstractions.Repositories;

public interface IRoomRepository : IRepositoryBase<Room, Guid>
{
    Task<IEnumerable<Room>> GetAvailableRoomsAsync(TimeInterval timeInterval, CancellationToken cancellationToken = default);
    Task<Room?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
}