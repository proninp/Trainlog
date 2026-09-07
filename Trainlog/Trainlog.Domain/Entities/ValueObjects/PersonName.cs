using CSharpFunctionalExtensions;
using Trainlog.Shared;

namespace Trainlog.Domain.Entities.ValueObjects;

public sealed class PersonName : ValueObject
{
    private const int MinLength = 2;

    private const int MaxLength = 150;

    private const string FieldName = "Name";

    private PersonName(string name) => Name = name;

    public string Name { get; }

    public static Result<PersonName?, Errors> Create(string? name)
    {
        var normalized = name?.Trim();
        if (string.IsNullOrEmpty(normalized))
        {
            return Result.Success<PersonName?, Errors>(null);
        }

        var errors = new List<Error>();

        var lengthResult = FieldValidator.ValidateStringField(normalized, FieldName, MinLength, MaxLength);
        if (lengthResult.IsFailure)
            errors.AddRange(lengthResult.Error);

        var charsResult = FieldValidator.ValidateAllowedNameChars(normalized, FieldName);
        if (charsResult.IsFailure)
            errors.AddRange(charsResult.Error);

        if (errors.Count > 0)
            return Result.Failure<PersonName?, Errors>(errors.ToErrors());

        return new PersonName(normalized);
    }

    public override string ToString() => Name;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Name;
    }
}
