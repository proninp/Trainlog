using System.Text.Json.Serialization;

namespace Trainlog.Error;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ErrorType
{
    None,
    Validation,
    NotFound,
    Failure,
    Conflict
}