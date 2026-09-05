namespace Trainlog.Error;

public static class ErrorExtensions
{
    public static Errors ToErrors(this Error error) => new([error]);
    
    public static Errors ToErrors(this List<Error> errors) => new([..errors]);
}