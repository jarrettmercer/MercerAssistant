using MercerAssistant.Core.DTOs;
using MercerAssistant.Core.Entities;
using MercerAssistant.Core.Enums;
using MercerAssistant.Core.Interfaces;
using MercerAssistant.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MercerAssistant.Infrastructure.Services;

public class SchedulingService : ISchedulingService
{
    private readonly AppDbContext _db;
    private readonly ILogger<SchedulingService> _logger;

    public SchedulingService(AppDbContext db, ILogger<SchedulingService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<List<TimeSlotDto>> GetAvailableSlotsAsync(
        string providerId, DateOnly date, int durationMinutes = 30)
    {
        var dayOfWeek = date.DayOfWeek;
        var windows = await _db.AvailabilityWindows
            .Where(w => w.UserId == providerId && w.DayOfWeek == dayOfWeek && w.IsActive)
            .ToListAsync();

        if (windows.Count == 0)
            return [];

        var dateStart = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var dateEnd = date.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

        var existingAppointments = await _db.Appointments
            .Where(a => a.ProviderId == providerId
                     && a.StartTimeUtc >= dateStart
                     && a.StartTimeUtc <= dateEnd
                     && a.Status != AppointmentStatus.Cancelled)
            .OrderBy(a => a.StartTimeUtc)
            .ToListAsync();

        var slots = new List<TimeSlotDto>();
        foreach (var window in windows)
        {
            var slotStart = date.ToDateTime(window.StartTime, DateTimeKind.Utc);
            var windowEnd = date.ToDateTime(window.EndTime, DateTimeKind.Utc);

            while (slotStart.AddMinutes(durationMinutes) <= windowEnd)
            {
                var slotEnd = slotStart.AddMinutes(durationMinutes);
                var isBooked = existingAppointments.Any(a =>
                    a.StartTimeUtc < slotEnd && a.EndTimeUtc > slotStart);

                slots.Add(new TimeSlotDto(slotStart, slotEnd, durationMinutes, !isBooked));
                slotStart = slotEnd;
            }
        }

        return slots;
    }

    public async Task<List<TimeSlotDto>> GetAvailableSlotsRangeAsync(
        string providerId, DateOnly startDate, DateOnly endDate, int durationMinutes = 30)
    {
        var allSlots = new List<TimeSlotDto>();
        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            var daySlots = await GetAvailableSlotsAsync(providerId, date, durationMinutes);
            allSlots.AddRange(daySlots);
        }
        return allSlots;
    }

    public async Task<Appointment> CreateBookingAsync(BookingRequestDto request)
    {
        var endTime = request.StartTimeUtc.AddMinutes(request.DurationMinutes);
        var isAvailable = await IsSlotAvailableAsync(request.ProviderId, request.StartTimeUtc, endTime);

        if (!isAvailable)
            throw new InvalidOperationException("The requested time slot is no longer available.");

        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            ProviderId = request.ProviderId,
            ClientName = request.ClientName,
            ClientEmail = request.ClientEmail,
            ClientPhone = request.ClientPhone,
            StartTimeUtc = request.StartTimeUtc,
            EndTimeUtc = endTime,
            DurationMinutes = request.DurationMinutes,
            Notes = request.Notes,
            BookingPageId = request.BookingPageId,
            Status = AppointmentStatus.Confirmed
        };

        _db.Appointments.Add(appointment);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Booking created: {Id} for {Client} at {Time}",
            appointment.Id, appointment.ClientName, appointment.StartTimeUtc);

        return appointment;
    }

    public async Task<Appointment> ConfirmBookingAsync(Guid appointmentId)
    {
        var appointment = await _db.Appointments.FindAsync(appointmentId)
            ?? throw new InvalidOperationException("Appointment not found.");

        appointment.Status = AppointmentStatus.Confirmed;
        appointment.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return appointment;
    }

    public async Task<Appointment> CancelBookingAsync(Guid appointmentId, string? reason = null)
    {
        var appointment = await _db.Appointments.FindAsync(appointmentId)
            ?? throw new InvalidOperationException("Appointment not found.");

        appointment.Status = AppointmentStatus.Cancelled;
        appointment.UpdatedAt = DateTime.UtcNow;
        if (reason is not null)
            appointment.Notes = (appointment.Notes ?? "") + $"\nCancellation reason: {reason}";

        await _db.SaveChangesAsync();

        return appointment;
    }

    public async Task<List<AppointmentDto>> GetUpcomingAppointmentsAsync(string providerId, int count = 10)
    {
        return await _db.Appointments
            .Where(a => a.ProviderId == providerId
                     && a.StartTimeUtc >= DateTime.UtcNow
                     && a.Status != AppointmentStatus.Cancelled)
            .OrderBy(a => a.StartTimeUtc)
            .Take(count)
            .Select(a => new AppointmentDto(
                a.Id, a.ClientName, a.ClientEmail,
                a.StartTimeUtc, a.EndTimeUtc, a.DurationMinutes,
                a.Title, a.Status.ToString()))
            .ToListAsync();
    }

    public async Task<List<AppointmentDto>> GetAppointmentsByDateRangeAsync(
        string providerId, DateTime startUtc, DateTime endUtc)
    {
        return await _db.Appointments
            .Where(a => a.ProviderId == providerId
                     && a.StartTimeUtc >= startUtc
                     && a.StartTimeUtc <= endUtc)
            .OrderBy(a => a.StartTimeUtc)
            .Select(a => new AppointmentDto(
                a.Id, a.ClientName, a.ClientEmail,
                a.StartTimeUtc, a.EndTimeUtc, a.DurationMinutes,
                a.Title, a.Status.ToString()))
            .ToListAsync();
    }

    public async Task<bool> IsSlotAvailableAsync(string providerId, DateTime startUtc, DateTime endUtc)
    {
        return !await _db.Appointments.AnyAsync(a =>
            a.ProviderId == providerId
            && a.Status != AppointmentStatus.Cancelled
            && a.StartTimeUtc < endUtc
            && a.EndTimeUtc > startUtc);
    }
}
