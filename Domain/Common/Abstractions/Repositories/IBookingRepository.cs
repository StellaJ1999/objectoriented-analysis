using Domain.Bookings;
using Domain.Common.Abstractions.Base;
using Domain.Common.ValueObjects;

namespace Domain.Common.Abstractions.Repositories;

public interface IBookingRepository : IRepositoryBase<Booking, Guid>
{
    Task<IEnumerable<Booking>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Booking>> GetByRoomIdAsync(Guid roomId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Booking>> GetConflictingBookingsAsync(Guid roomId, TimeInterval timeInterval, CancellationToken cancellationToken = default);
    Task<IEnumerable<Booking>> GetUpcomingBookingsAsync(DateTime from, CancellationToken cancellationToken = default);
}