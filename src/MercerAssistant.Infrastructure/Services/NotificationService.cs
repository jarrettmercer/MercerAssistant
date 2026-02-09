using MercerAssistant.Core.Entities;
using MercerAssistant.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace MercerAssistant.Infrastructure.Services;

/// <summary>
/// Stub notification service. SendGrid integration added in Phase 6.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(ILogger<NotificationService> logger)
    {
        _logger = logger;
    }

    public Task SendBookingConfirmationAsync(Appointment appointment)
    {
        _logger.LogInformation("[Stub] Booking confirmation for {Client} at {Time}",
            appointment.ClientName, appointment.StartTimeUtc);
        return Task.CompletedTask;
    }

    public Task SendBookingCancellationAsync(Appointment appointment)
    {
        _logger.LogInformation("[Stub] Booking cancellation for {Client}", appointment.ClientName);
        return Task.CompletedTask;
    }

    public Task SendBookingReminderAsync(Appointment appointment)
    {
        _logger.LogInformation("[Stub] Booking reminder for {Client}", appointment.ClientName);
        return Task.CompletedTask;
    }
}
