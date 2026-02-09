using MercerAssistant.Core.Entities;
using MercerAssistant.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace MercerAssistant.Infrastructure.Services;

/// <summary>
/// Stub Google Calendar service. Full OAuth2 integration added in Phase 5.
/// </summary>
public class GoogleCalendarService : ICalendarProvider
{
    private readonly ILogger<GoogleCalendarService> _logger;

    public GoogleCalendarService(ILogger<GoogleCalendarService> logger)
    {
        _logger = logger;
    }

    public Task<string> CreateEventAsync(Appointment appointment)
    {
        _logger.LogInformation("[Stub] Would create Google Calendar event for {Client}", appointment.ClientName);
        return Task.FromResult(string.Empty);
    }

    public Task UpdateEventAsync(Appointment appointment)
    {
        _logger.LogInformation("[Stub] Would update Google Calendar event");
        return Task.CompletedTask;
    }

    public Task DeleteEventAsync(string externalEventId)
    {
        _logger.LogInformation("[Stub] Would delete Google Calendar event {Id}", externalEventId);
        return Task.CompletedTask;
    }

    public Task<List<(DateTime Start, DateTime End)>> GetBusyTimesAsync(DateTime startUtc, DateTime endUtc)
    {
        return Task.FromResult(new List<(DateTime Start, DateTime End)>());
    }

    public Task<string> GetAuthorizationUrlAsync(string userId)
    {
        return Task.FromResult("#google-calendar-not-configured");
    }

    public Task HandleOAuthCallbackAsync(string code, string userId)
    {
        _logger.LogInformation("[Stub] Would handle Google OAuth callback");
        return Task.CompletedTask;
    }

    public Task<bool> IsConnectedAsync(string userId)
    {
        return Task.FromResult(false);
    }
}
