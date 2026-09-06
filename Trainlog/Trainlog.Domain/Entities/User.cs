using CSharpFunctionalExtensions;
using Trainlog.Domain.Entities.Base;
using Trainlog.Domain.Enums;
using Trainlog.Shared;

namespace Trainlog.Domain.Entities;

public sealed class User : SoftDeletableEntity
{
    private const int NameMinLength = 2;

    private const int NameMaxLength = 150;

    private const int AgeMin = 3;

    private const int AgeMax = 99;

    private const decimal WeightMin = 20;

    private const decimal WeightMax = 301;

    private const int HealthNotesMinLength = 3;

    private const int HealthNotesMaxLength = 500;

    private User(
        UserProfileDraft userProfileDraft,
        Guid? id = null)
    {
        Id = id ?? Guid.CreateVersion7();
        Name = userProfileDraft.Name;
        Age = userProfileDraft.Age;
        BodyWeightKg = userProfileDraft.BodyWeightKg;
        Experience = userProfileDraft.Experience;
        Goal = userProfileDraft.Goal;
        HealthNotes = userProfileDraft.HealthNotes;
    }

    public string? Name { get; private set; }

    public short? Age { get; private set; }

    public decimal? BodyWeightKg { get; private set; }

    public ExperienceLevel? Experience { get; private set; }

    public TrainingGoal? Goal { get; private set; }

    public string? HealthNotes { get; private set; }

    public static Result<User, Errors> Create(
        string? name,
        short? age,
        decimal? bodyWeightKg,
        ExperienceLevel? experience,
        TrainingGoal? goal,
        string? healthNotes)
    {
        var userDraft = new UserProfileDraft(name, age, bodyWeightKg, experience, goal, healthNotes);
        userDraft = NormalizeFields(userDraft);
        var userValidationResult = ValidateUser(userDraft);
        if (userValidationResult.IsFailure)
            return Result.Failure<User, Errors>(userValidationResult.Error);

        return new User(userDraft);
    }

    public UnitResult<Errors> ChangeName(string? name)
    {
        name = NormalizeStringField(name);
        var nameErrors = ValidateName(name);
        if (nameErrors.Count > 0)
            return UnitResult.Failure(nameErrors.ToErrors());
        Name = name;
        Touch();
        return UnitResult.Success<Errors>();
    }

    public UnitResult<Errors> ChangeAge(short? age)
    {
        var ageErrors = ValidateAge(age);
        if (ageErrors.Count > 0)
            return UnitResult.Failure(ageErrors.ToErrors());
        Age = age;
        Touch();
        return UnitResult.Success<Errors>();
    }

    public UnitResult<Errors> ChangeBodyWeight(decimal? bodyWeightKg)
    {
        var bodyWeightErrors = ValidateBodyWeight(bodyWeightKg);
        if (bodyWeightErrors.Count > 0)
            return UnitResult.Failure(bodyWeightErrors.ToErrors());
        BodyWeightKg = bodyWeightKg;
        Touch();
        return UnitResult.Success<Errors>();
    }

    public UnitResult<Errors> ChangeExperience(ExperienceLevel? experience)
    {
        var experienceErrors = ValidateExperience(experience);
        if (experienceErrors.Count > 0)
            return UnitResult.Failure(experienceErrors.ToErrors());
        Experience = experience;
        Touch();
        return UnitResult.Success<Errors>();
    }

    public UnitResult<Errors> ChangeGoal(TrainingGoal? goal)
    {
        var goalErrors = ValidateGoal(goal);
        if (goalErrors.Count > 0)
            return UnitResult.Failure(goalErrors.ToErrors());
        Goal = goal;
        Touch();
        return UnitResult.Success<Errors>();
    }

    public UnitResult<Errors> ChangeHealthNotes(string? healthNotes)
    {
        healthNotes = NormalizeStringField(healthNotes);
        var healthNotesErrors = ValidateHealthNotes(healthNotes);
        if (healthNotesErrors.Count > 0)
            return UnitResult.Failure(healthNotesErrors.ToErrors());
        HealthNotes = healthNotes;
        Touch();
        return UnitResult.Success<Errors>();
    }

    private static UnitResult<Errors> ValidateUser(
        UserProfileDraft userProfileDraft)
    {
        var userErrors = new List<Error>();

        userErrors.AddRange(ValidateName(userProfileDraft.Name));
        userErrors.AddRange(ValidateAge(userProfileDraft.Age));
        userErrors.AddRange(ValidateBodyWeight(userProfileDraft.BodyWeightKg));
        userErrors.AddRange(ValidateExperience(userProfileDraft.Experience));
        userErrors.AddRange(ValidateGoal(userProfileDraft.Goal));
        userErrors.AddRange(ValidateHealthNotes(userProfileDraft.HealthNotes));

        return userErrors.Count == 0
            ? UnitResult.Success<Errors>()
            : UnitResult.Failure(userErrors.ToErrors());
    }

    private static List<Error> ValidateName(string? name)
    {
        var errors = new List<Error>();
        if (name is null)
            return errors;
        var nameValidationResult =
            FieldValidator.ValidateStringField(name, nameof(Name), NameMinLength, NameMaxLength);
        if (nameValidationResult.IsFailure)
            errors.AddRange(nameValidationResult.Error);
        var nameCharsValidationResult = FieldValidator.ValidateAllowedNameChars(name, nameof(Name));
        if (nameCharsValidationResult.IsFailure)
            errors.AddRange(nameCharsValidationResult.Error);

        return errors;
    }

    private static List<Error> ValidateAge(short? age)
    {
        var errors = new List<Error>();
        if (age is < AgeMin or > AgeMax)
        {
            errors.Add(GeneralErrors.ValueIsInvalid(nameof(Age),
                $"Age must be between {AgeMin} and {AgeMax}"));
        }

        return errors;
    }

    private static List<Error> ValidateBodyWeight(decimal? bodyWeightKg)
    {
        var errors = new List<Error>();
        if (bodyWeightKg is < WeightMin or > WeightMax)
        {
            errors.Add(GeneralErrors.ValueIsInvalid(nameof(BodyWeightKg),
                $"Weight must be between {WeightMin} and {WeightMax}"));
        }

        return errors;
    }

    private static List<Error> ValidateExperience(ExperienceLevel? experience)
    {
        var errors = new List<Error>();
        if (experience is not null && !Enum.IsDefined(experience.Value))
        {
            errors.Add(GeneralErrors.ValueIsInvalid(nameof(Experience)));
        }

        return errors;
    }

    private static List<Error> ValidateGoal(TrainingGoal? goal)
    {
        var errors = new List<Error>();
        if (goal is not null && !Enum.IsDefined(goal.Value))
        {
            errors.Add(GeneralErrors.ValueIsInvalid(nameof(Goal)));
        }

        return errors;
    }

    private static List<Error> ValidateHealthNotes(string? healthNotes)
    {
        var errors = new List<Error>();
        if (healthNotes is null)
            return errors;
        var healthNotesValidationResult =
            FieldValidator.ValidateStringField(healthNotes, nameof(HealthNotes), HealthNotesMinLength,
                HealthNotesMaxLength);
        if (healthNotesValidationResult.IsFailure)
            errors.AddRange(healthNotesValidationResult.Error);

        return errors;
    }

    private static UserProfileDraft NormalizeFields(UserProfileDraft draft)
    {
        return draft with
        {
            Name = NormalizeStringField(draft.Name), HealthNotes = NormalizeStringField(draft.HealthNotes)
        };
    }

    private static string? NormalizeStringField(string? field)
    {
        field = field?.Trim();
        if (field?.Length == 0)
            field = null;
        return field;
    }

    private sealed record UserProfileDraft(
        string? Name,
        short? Age,
        decimal? BodyWeightKg,
        ExperienceLevel? Experience,
        TrainingGoal? Goal,
        string? HealthNotes
    );
}