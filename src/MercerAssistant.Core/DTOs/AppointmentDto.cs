namespace MercerAssistant.Core.DTOs;

public record AppointmentDto(
    Guid Id,
    string ClientName,
    string ClientEmail,
    DateTime StartTimeUtc,
    DateTime EndTimeUtc,
    int DurationMinutes,
    string? Title,
    string Status);
