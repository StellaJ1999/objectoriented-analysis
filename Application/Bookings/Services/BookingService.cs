using Application.Bookings.Abstractions;
using Application.Bookings.Dtos;
using Domain.Bookings;
using Domain.Common.Abstractions.Repositories;
using Domain.Common.ValueObjects;

namespace Application.Bookings.Services;

public sealed class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IRoomRepository _roomRepository;
    private readonly IUserRepository _userRepository;

    public BookingService(
        IBookingRepository bookingRepository,
        IRoomRepository roomRepository,
        IUserRepository userRepository)
    {
        _bookingRepository = bookingRepository;
        _roomRepository = roomRepository;
        _userRepository = userRepository;
    }

    public async Task<BookingResponseDto> CreateBookingAsync(CreateBookingDto dto, Guid currentUserId, CancellationToken ct = default)
    {
        // Validera att användaren finns och är aktiv
        var user = await _userRepository.GetByIdAsync(currentUserId, ct);
        if (user == null || !user.IsActive)
            throw new InvalidOperationException("Användaren finns inte eller är inaktiv");

        // Validera att rummet finns 
        var room = await _roomRepository.GetByIdAsync(dto.RoomId, ct);
        if (room == null || !room.IsActive)
            throw new InvalidOperationException("Rummet finns inte eller är inaktivt");

        // Skapa TimeInterval
        var timeInterval = new TimeInterval(dto.StartTime, dto.EndTime);

        // Kontrollera överlapp av bokningar
        var conflictingBookings = await _bookingRepository.GetConflictingBookingsAsync(
            dto.RoomId,
            timeInterval,
            ct);

        if (conflictingBookings.Any())
            throw new InvalidOperationException("Rummet är redan bokat under den angivna tiden");

        // Skapa bokning
        var booking = new Booking(dto.RoomId, currentUserId, timeInterval, dto.Purpose);

        // Spara i databas
        await _bookingRepository.CreateAsync(booking, ct);

        // Returnera DTO
        return MapToDto(booking, room.Name, user.FullName);
    }

    public async Task<BookingResponseDto> CreateBookingForUserAsync(
        CreateBookingDto dto,
        Guid receptionistId,
        Guid bookedForUserId,
        CancellationToken ct = default)
    {
        // Validera att receptionisten har rätt behörighet
        var receptionist = await _userRepository.GetByIdAsync(receptionistId, ct);
        if (receptionist == null || !receptionist.CanBookForOthers())
            throw new UnauthorizedAccessException("Endast receptionister kan boka åt andra");

        // Använd samma logik som CreateBookingAsync men med bookedForUserId
        var user = await _userRepository.GetByIdAsync(bookedForUserId, ct);
        if (user == null || !user.IsActive)
            throw new InvalidOperationException("Användaren som ska bokas för finns inte eller är inaktiv");

        var room = await _roomRepository.GetByIdAsync(dto.RoomId, ct);
        if (room == null || !room.IsActive)
            throw new InvalidOperationException("Rummet finns inte eller är inaktivt");

        var timeInterval = new TimeInterval(dto.StartTime, dto.EndTime);

        var conflictingBookings = await _bookingRepository.GetConflictingBookingsAsync(
            dto.RoomId,
            timeInterval,
            ct);

        if (conflictingBookings.Any())
            throw new InvalidOperationException("Rummet är redan bokat under den angivna tiden");

        var booking = new Booking(dto.RoomId, bookedForUserId, timeInterval, dto.Purpose);

        await _bookingRepository.CreateAsync(booking, ct);

        return MapToDto(booking, room.Name, user.FullName);
    }

    public async Task<BookingResponseDto> UpdateBookingAsync(
        Guid bookingId,
        UpdateBookingDto dto,
        Guid currentUserId,
        CancellationToken ct = default)
    {
        // Hämta existerande bokning
        var booking = await _bookingRepository.GetByIdAsync(bookingId, ct);
        if (booking == null)
            throw new InvalidOperationException("Bokningen finns inte");

        // Validera ägarskap
        if (!booking.IsOwnedBy(currentUserId))
        {
            var user = await _userRepository.GetByIdAsync(currentUserId, ct);
            if (user == null || !user.CanBookForOthers())
                throw new UnauthorizedAccessException("Du har inte behörighet att ändra denna bokning");
        }

        // Skapa nytt TimeInterval
        var newTimeInterval = new TimeInterval(dto.StartTime, dto.EndTime);

        // Kontrollera överlapp 
        var conflictingBookings = await _bookingRepository.GetConflictingBookingsAsync(
            booking.RoomId,
            newTimeInterval,
            ct);

        if (conflictingBookings.Any(b => b.Id != bookingId))
            throw new InvalidOperationException("Rummet är redan bokat under den nya tiden");

        // Uppdatera bokning
        booking.Reschedule(newTimeInterval);

        await _bookingRepository.UpdateAsync(bookingId, booking, ct);

        // Hämta room och user info för response
        var room = await _roomRepository.GetByIdAsync(booking.RoomId, ct);
        var bookedUser = await _userRepository.GetByIdAsync(booking.UserId, ct);

        return MapToDto(booking, room?.Name ?? "", bookedUser?.FullName ?? "");
    }

    public async Task<bool> CancelBookingAsync(Guid bookingId, Guid currentUserId, CancellationToken ct = default)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId, ct);
        if (booking == null)
            throw new InvalidOperationException("Bokningen finns inte");

        // Validera ägarskap
        if (!booking.IsOwnedBy(currentUserId))
        {
            var user = await _userRepository.GetByIdAsync(currentUserId, ct);
            if (user == null || !user.CanBookForOthers())
                throw new UnauthorizedAccessException("Du har inte behörighet att avboka denna bokning");
        }

        // Avboka
        booking.Cancel();

        return await _bookingRepository.UpdateAsync(bookingId, booking, ct);
    }

    public async Task<IEnumerable<BookingResponseDto>> GetUserBookingsAsync(Guid userId, CancellationToken ct = default)
    {
        var bookings = await _bookingRepository.GetByUserIdAsync(userId, ct);

        var bookingDtos = new List<BookingResponseDto>();
        foreach (var booking in bookings)
        {
            var room = await _roomRepository.GetByIdAsync(booking.RoomId, ct);
            var user = await _userRepository.GetByIdAsync(booking.UserId, ct);
            bookingDtos.Add(MapToDto(booking, room?.Name ?? "", user?.FullName ?? ""));
        }

        return bookingDtos;
    }

    public async Task<IEnumerable<BookingResponseDto>> GetBookingHistoryAsync(DateTime? fromDate = null, CancellationToken ct = default)
    {
        var from = fromDate ?? DateTime.UtcNow.AddMonths(-3); // Default 3 månader tillbaka
        var bookings = await _bookingRepository.GetUpcomingBookingsAsync(from, ct);

        var bookingDtos = new List<BookingResponseDto>();
        foreach (var booking in bookings)
        {
            var room = await _roomRepository.GetByIdAsync(booking.RoomId, ct);
            var user = await _userRepository.GetByIdAsync(booking.UserId, ct);
            bookingDtos.Add(MapToDto(booking, room?.Name ?? "", user?.FullName ?? ""));
        }

        return bookingDtos;
    }

    public async Task<IEnumerable<BookingResponseDto>> GetRoomBookingsAsync(Guid roomId, DateTime date, CancellationToken ct = default)
    {
        var bookings = await _bookingRepository.GetByRoomIdAsync(roomId, ct);

        // Filtrera på datum
        var dateBookings = bookings.Where(b =>
            b.TimeInterval.StartTime.Date == date.Date &&
            b.Status == "Active");

        var bookingDtos = new List<BookingResponseDto>();
        var room = await _roomRepository.GetByIdAsync(roomId, ct);

        foreach (var booking in dateBookings)
        {
            var user = await _userRepository.GetByIdAsync(booking.UserId, ct);
            bookingDtos.Add(MapToDto(booking, room?.Name ?? "", user?.FullName ?? ""));
        }

        return bookingDtos;
    }

    public async Task<BookingResponseDto?> GetBookingByIdAsync(Guid bookingId, CancellationToken ct = default)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId, ct);
        if (booking == null)
            return null;

        var room = await _roomRepository.GetByIdAsync(booking.RoomId, ct);
        var user = await _userRepository.GetByIdAsync(booking.UserId, ct);

        return MapToDto(booking, room?.Name ?? "", user?.FullName ?? "");
    }

    private static BookingResponseDto MapToDto(Booking booking, string roomName, string userFullName)
    {
        return new BookingResponseDto
        {
            Id = booking.Id,
            RoomId = booking.RoomId,
            RoomName = roomName,
            UserId = booking.UserId,
            UserFullName = userFullName,
            StartTime = booking.TimeInterval.StartTime,
            EndTime = booking.TimeInterval.EndTime,
            Purpose = booking.Purpose,
            Status = booking.Status
        };
    }
}