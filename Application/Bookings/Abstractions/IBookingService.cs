using Application.Bookings.Dtos;

namespace Application.Bookings.Abstractions;

public interface IBookingService
{
    // UC-4: Skapa bokning
    Task<BookingResponseDto> CreateBookingAsync(CreateBookingDto dto, Guid currentUserId, CancellationToken ct = default);

    // UC-7: Receptionist bokar åt annan
    Task<BookingResponseDto> CreateBookingForUserAsync(CreateBookingDto dto, Guid receptionistId, Guid bookedForUserId, CancellationToken ct = default);

    // UC-5: Ändra egen bokning
    Task<BookingResponseDto> UpdateBookingAsync(Guid bookingId, UpdateBookingDto dto, Guid currentUserId, CancellationToken ct = default);

    // UC-6: Avboka egen bokning
    Task<bool> CancelBookingAsync(Guid bookingId, Guid currentUserId, CancellationToken ct = default);

    // Visa egna bokningar
    Task<IEnumerable<BookingResponseDto>> GetUserBookingsAsync(Guid userId, CancellationToken ct = default);

    // UC-9: Visa historik
    Task<IEnumerable<BookingResponseDto>> GetBookingHistoryAsync(DateTime? fromDate = null, CancellationToken ct = default);

    // UC-2: Visa kalendeöversikt för ett rum
    Task<IEnumerable<BookingResponseDto>> GetRoomBookingsAsync(Guid roomId, DateTime date, CancellationToken ct = default);

    // Hämta specifik bokning
    Task<BookingResponseDto?> GetBookingByIdAsync(Guid bookingId, CancellationToken ct = default);
}