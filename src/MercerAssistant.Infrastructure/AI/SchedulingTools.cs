using System.ComponentModel;
using MercerAssistant.Core.DTOs;
using MercerAssistant.Core.Interfaces;

namespace MercerAssistant.Infrastructure.AI;

/// <summary>
/// Methods registered as AI tools via AIFunctionFactory.Create().
/// GPT 5.1 mini can call these functions during conversation.
/// </summary>
public class SchedulingTools
{
    private readonly ISchedulingService _scheduling;
    private readonly string _providerId;

    public SchedulingTools(ISchedulingService scheduling, string providerId)
    {
        _scheduling = scheduling;
        _providerId = providerId;
    }

    [Description("Check available time slots for a specific date. Returns a list of open slots.")]
    public async Task<string> CheckAvailability(
        [Description("The date to check in yyyy-MM-dd format")] string date,
        [Description("Duration in minutes (default 30)")] int durationMinutes = 30)
    {
        if (!DateOnly.TryParse(date, out var parsedDate))
            return "Invalid date format. Please use yyyy-MM-dd.";

        var slots = await _scheduling.GetAvailableSlotsAsync(_providerId, parsedDate, durationMinutes);

        if (slots.Count == 0)
            return $"No available slots on {parsedDate:dddd, MMMM d, yyyy}.";

        var available = slots.Where(s => s.IsAvailable).ToList();
        if (available.Count == 0)
            return $"All slots are booked on {parsedDate:dddd, MMMM d, yyyy}.";

        var lines = available.Select(s => $"  {s.StartUtc:h:mm tt} - {s.EndUtc:h:mm tt}");
        return $"Available slots on {parsedDate:dddd, MMMM d, yyyy}:\n{string.Join("\n", lines)}";
    }

    [Description("Create a new booking/appointment")]
    public async Task<string> CreateBooking(
        [Description("Client's full name")] string clientName,
        [Description("Client's email address")] string clientEmail,
        [Description("Start time in ISO 8601 format (yyyy-MM-ddTHH:mm:ss)")] string startTime,
        [Description("Duration in minutes (default 30)")] int durationMinutes = 30,
        [Description("Optional notes about the appointment")] string? notes = null)
    {
        if (!DateTime.TryParse(startTime, out var parsedStart))
            return "Invalid date/time format. Please use ISO 8601 format.";

        var request = new BookingRequestDto(
            _providerId, clientName, clientEmail, null,
            parsedStart.ToUniversalTime(), durationMinutes, notes, null);

        try
        {
            var appointment = await _scheduling.CreateBookingAsync(request);
            return $"Booking created successfully!\n" +
                   $"  ID: {appointment.Id}\n" +
                   $"  Client: {appointment.ClientName}\n" +
                   $"  Time: {appointment.StartTimeUtc:f} - {appointment.EndTimeUtc:t}\n" +
                   $"  Status: {appointment.Status}";
        }
        catch (InvalidOperationException ex)
        {
            return $"Could not create booking: {ex.Message}";
        }
    }

    [Description("Cancel an existing appointment by its ID")]
    public async Task<string> CancelBooking(
        [Description("The appointment ID (GUID)")] string appointmentId,
        [Description("Optional reason for cancellation")] string? reason = null)
    {
        if (!Guid.TryParse(appointmentId, out var id))
            return "Invalid appointment ID format.";

        try
        {
            var appointment = await _scheduling.CancelBookingAsync(id, reason);
            return $"Appointment cancelled successfully.\n" +
                   $"  Client: {appointment.ClientName}\n" +
                   $"  Was scheduled for: {appointment.StartTimeUtc:f}";
        }
        catch (Exception ex)
        {
            return $"Could not cancel appointment: {ex.Message}";
        }
    }

    [Description("List upcoming appointments. Returns the next appointments in chronological order.")]
    public async Task<string> ListUpcomingAppointments(
        [Description("Number of appointments to return (default 5)")] int count = 5)
    {
        var appointments = await _scheduling.GetUpcomingAppointmentsAsync(_providerId, count);

        if (appointments.Count == 0)
            return "No upcoming appointments.";

        var lines = appointments.Select(a =>
            $"  {a.StartTimeUtc:ddd MMM d, h:mm tt} - {a.ClientName} ({a.Status})" +
            (a.Title is not null ? $" - {a.Title}" : ""));

        return $"Upcoming appointments:\n{string.Join("\n", lines)}";
    }
}
