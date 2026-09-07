using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using Trainlog.Shared;

namespace Trainlog.Domain;

internal static partial class FieldValidator
{
    
    [GeneratedRegex(@"^\p{L}+(?:[ '\u2019\-]\p{L}+)*$", RegexOptions.CultureInvariant)]
    private static partial Regex AllowedNameRegex();
    
    public static UnitResult<Errors> ValidateStringField(string fieldValue, string fieldName, int minLength,
        int maxLength)
    {
        
        if (string.IsNullOrWhiteSpace(fieldValue))
        {
            return UnitResult.Failure(GeneralErrors.ValueIsInvalid(
                    fieldName, "The field cannot consist only of spaces")
                .ToErrors());
        }

        if (fieldValue.Length < minLength)
            return UnitResult.Failure(GeneralErrors.InvalidFieldLength(fieldName, minLength: minLength).ToErrors());

        if (maxLength > 0 && fieldValue.Length > maxLength)
            return UnitResult.Failure(GeneralErrors.InvalidFieldLength(fieldName, maxLength: maxLength).ToErrors());

        return UnitResult.Success<Errors>();
    }
    
    public static UnitResult<Errors> ValidateAllowedNameChars(string fieldValue, string fieldName)
    {
        return !AllowedNameRegex().IsMatch(fieldValue)
            ? UnitResult.Failure(
                GeneralErrors.ValueIsInvalid(fieldName)
                    .ToErrors())
            : UnitResult.Success<Errors>();
    }
}