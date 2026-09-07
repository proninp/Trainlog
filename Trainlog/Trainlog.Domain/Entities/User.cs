using System.Collections.ObjectModel;
using CSharpFunctionalExtensions;
using Trainlog.Domain.Entities.Base;
using Trainlog.Domain.Entities.ValueObjects;
using Trainlog.Domain.Enums;
using Trainlog.Shared;

namespace Trainlog.Domain.Entities;

public sealed class User : SoftDeletableEntity
{
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

    public PersonName? Name { get; private set; }

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
        var personNameResult = PersonName.Create(name);
        if (personNameResult.IsFailure)
            return personNameResult.Error;
        
        var userDraft = new UserProfileDraft(
            personNameResult.Value, age, bodyWeightKg, experience, goal, healthNotes);
        userDraft = NormalizeFields(userDraft);

        var userValidationResult = ValidateUser(userDraft);
        if (userValidationResult.IsFailure)
            return Result.Failure<User, Errors>(userValidationResult.Error);

        return new User(userDraft);
    }

    public static Result<User, Errors> Create()
    {
        var userDraft = new UserProfileDraft(
            null, null, null, null, null, null);
        return new User(userDraft);
    }

    public UnitResult<Errors> ChangeName(string? name)
    {
        var personNameResult = PersonName.Create(name);
        if (personNameResult.IsFailure)
            return personNameResult.Error;
        Name = personNameResult.Value;
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

        userErrors.AddRange(ValidateAge(userProfileDraft.Age));
        userErrors.AddRange(ValidateBodyWeight(userProfileDraft.BodyWeightKg));
        userErrors.AddRange(ValidateExperience(userProfileDraft.Experience));
        userErrors.AddRange(ValidateGoal(userProfileDraft.Goal));
        userErrors.AddRange(ValidateHealthNotes(userProfileDraft.HealthNotes));

        return userErrors.Count == 0
            ? UnitResult.Success<Errors>()
            : UnitResult.Failure(userErrors.ToErrors());
    }

    private static ReadOnlyCollection<Error> ValidateAge(short? age)
    {
        var errors = new List<Error>();
        if (age is < AgeMin or > AgeMax)
        {
            errors.Add(GeneralErrors.ValueIsInvalid(nameof(Age),
                $"Age must be between {AgeMin} and {AgeMax}"));
        }

        return errors.AsReadOnly();
    }

    private static ReadOnlyCollection<Error> ValidateBodyWeight(decimal? bodyWeightKg)
    {
        var errors = new List<Error>();
        if (bodyWeightKg is < WeightMin or > WeightMax)
        {
            errors.Add(GeneralErrors.ValueIsInvalid(nameof(BodyWeightKg),
                $"Weight must be between {WeightMin} and {WeightMax}"));
        }

        return errors.AsReadOnly();
    }

    private static ReadOnlyCollection<Error> ValidateExperience(ExperienceLevel? experience)
    {
        var errors = new List<Error>();
        if (experience is not null && !Enum.IsDefined(experience.Value))
        {
            errors.Add(GeneralErrors.ValueIsInvalid(nameof(Experience)));
        }

        return errors.AsReadOnly();
    }

    private static ReadOnlyCollection<Error> ValidateGoal(TrainingGoal? goal)
    {
        var errors = new List<Error>();
        if (goal is not null && !Enum.IsDefined(goal.Value))
        {
            errors.Add(GeneralErrors.ValueIsInvalid(nameof(Goal)));
        }

        return errors.AsReadOnly();
    }

    private static ReadOnlyCollection<Error> ValidateHealthNotes(string? healthNotes)
    {
        var errors = new List<Error>();
        if (healthNotes is null)
            return errors.AsReadOnly();
        var healthNotesValidationResult =
            FieldValidator.ValidateStringField(healthNotes, nameof(HealthNotes), HealthNotesMinLength,
                HealthNotesMaxLength);
        if (healthNotesValidationResult.IsFailure)
            errors.AddRange(healthNotesValidationResult.Error);

        return errors.AsReadOnly();
    }

    private static UserProfileDraft NormalizeFields(UserProfileDraft draft)
    {
        return draft with { HealthNotes = NormalizeStringField(draft.HealthNotes) };
    }

    private static string? NormalizeStringField(string? field)
    {
        field = field?.Trim();
        if (field?.Length == 0)
            field = null;
        return field;
    }

    private sealed record UserProfileDraft(
        PersonName? Name,
        short? Age,
        decimal? BodyWeightKg,
        ExperienceLevel? Experience,
        TrainingGoal? Goal,
        string? HealthNotes
    );
}