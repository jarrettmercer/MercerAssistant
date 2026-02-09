using MercerAssistant.Core.Entities;

namespace MercerAssistant.Core.Interfaces;

public interface ICalendarProvider
{
    Task<string> CreateEventAsync(Appointment appointment);
    Task UpdateEventAsync(Appointment appointment);
    Task DeleteEventAsync(string externalEventId);
    Task<List<(DateTime Start, DateTime End)>> GetBusyTimesAsync(DateTime startUtc, DateTime endUtc);
    Task<string> GetAuthorizationUrlAsync(string userId);
    Task HandleOAuthCallbackAsync(string code, string userId);
    Task<bool> IsConnectedAsync(string userId);
}
