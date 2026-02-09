namespace MercerAssistant.Core.DTOs;

public record ChatResponseDto(
    string Message,
    bool ToolWasInvoked,
    string? ToolName);
