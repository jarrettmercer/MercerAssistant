namespace MercerAssistant.Core.DTOs;

public record BookingRequestDto(
    string ProviderId,
    string ClientName,
    string ClientEmail,
    string? ClientPhone,
    DateTime StartTimeUtc,
    int DurationMinutes,
    string? Notes,
    Guid? BookingPageId);
