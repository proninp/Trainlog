using System.Text.Json.Serialization;

namespace Trainlog.Shared;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ErrorType
{
    None,
    Validation,
    NotFound,
    Failure,
    Conflict
}