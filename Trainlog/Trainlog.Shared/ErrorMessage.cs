namespace Trainlog.Error;

public record ErrorMessage(string Code, string Message, string? InvalidField = null);