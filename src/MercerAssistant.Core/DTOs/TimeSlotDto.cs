namespace MercerAssistant.Core.DTOs;

public record TimeSlotDto(
    DateTime StartUtc,
    DateTime EndUtc,
    int DurationMinutes,
    bool IsAvailable);
