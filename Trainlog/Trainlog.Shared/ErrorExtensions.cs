namespace Trainlog.Shared;

public static class ErrorExtensions
{
    public static Errors ToErrors(this Error error) => new([error]);
    
    public static Errors ToErrors(this IEnumerable<Error> errors) => new([..errors]);
}