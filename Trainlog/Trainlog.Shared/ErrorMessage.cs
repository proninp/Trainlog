namespace Trainlog.Shared;

public record ErrorMessage(string Code, string Message, string? InvalidField = null);