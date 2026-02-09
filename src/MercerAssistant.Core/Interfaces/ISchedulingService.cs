using MercerAssistant.Core.DTOs;
using MercerAssistant.Core.Entities;

namespace MercerAssistant.Core.Interfaces;

public interface ISchedulingService
{
    Task<List<TimeSlotDto>> GetAvailableSlotsAsync(string providerId, DateOnly date, int durationMinutes = 30);
    Task<List<TimeSlotDto>> GetAvailableSlotsRangeAsync(string providerId, DateOnly startDate, DateOnly endDate, int durationMinutes = 30);
    Task<Appointment> CreateBookingAsync(BookingRequestDto request);
    Task<Appointment> ConfirmBookingAsync(Guid appointmentId);
    Task<Appointment> CancelBookingAsync(Guid appointmentId, string? reason = null);
    Task<List<AppointmentDto>> GetUpcomingAppointmentsAsync(string providerId, int count = 10);
    Task<List<AppointmentDto>> GetAppointmentsByDateRangeAsync(string providerId, DateTime startUtc, DateTime endUtc);
    Task<bool> IsSlotAvailableAsync(string providerId, DateTime startUtc, DateTime endUtc);
}
