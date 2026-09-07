using Trainlog.Domain.Entities;
using Trainlog.Domain.Enums;
using Trainlog.Shared;

namespace Trainlog.Domain.UnitTests;

public class UserTest
{
    private static User EmptyUser() => User.Create().Value;

    private static bool HasFieldError(Errors errors, string field) =>
        errors.Any(e => e.ErrorType == ErrorType.Validation && e.ErrorMessage.InvalidField == field);

    // ---------------------------------------------------------------------
    // Create() - empty user
    // ---------------------------------------------------------------------

    [Fact]
    public void CreateEmpty_ReturnsUserWithoutProfileData()
    {
        var result = User.Create();

        Assert.True(result.IsSuccess);
        var user = result.Value;
        Assert.Null(user.Name);
        Assert.Null(user.Age);
        Assert.Null(user.BodyWeightKg);
        Assert.Null(user.Experience);
        Assert.Null(user.Goal);
        Assert.Null(user.HealthNotes);
    }

    [Fact]
    public void CreateEmpty_AssignsIdentityAndAuditDefaults()
    {
        var user = User.Create().Value;

        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Null(user.UpdatedAt);
        Assert.Null(user.DeletedAt);
        Assert.True(user.CreatedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void CreateEmpty_GeneratesUniqueIds()
    {
        Assert.NotEqual(User.Create().Value.Id, User.Create().Value.Id);
    }

    // ---------------------------------------------------------------------
    // Create(...) - full factory
    // ---------------------------------------------------------------------

    [Fact]
    public void Create_AllArgumentsNull_Succeeds()
    {
        var result = User.Create(null, null, null, null, null, null);

        Assert.True(result.IsSuccess);
        var user = result.Value;
        Assert.Null(user.Name);
        Assert.Null(user.Age);
        Assert.Null(user.BodyWeightKg);
        Assert.Null(user.Experience);
        Assert.Null(user.Goal);
        Assert.Null(user.HealthNotes);
        Assert.Null(user.UpdatedAt);
    }

    [Fact]
    public void Create_AllFieldsValid_MapsEveryValue()
    {
        var result = User.Create(
            "Ivan", 30, 75.5m, ExperienceLevel.Intermediate, TrainingGoal.Strength, "Bad left knee");

        Assert.True(result.IsSuccess);
        var user = result.Value;
        Assert.Equal("Ivan", user.Name!.Name);
        Assert.Equal((short)30, user.Age);
        Assert.Equal(75.5m, user.BodyWeightKg);
        Assert.Equal(ExperienceLevel.Intermediate, user.Experience);
        Assert.Equal(TrainingGoal.Strength, user.Goal);
        Assert.Equal("Bad left knee", user.HealthNotes);
        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Null(user.UpdatedAt);
        Assert.Null(user.DeletedAt);
    }

    [Theory]
    [InlineData("  Ivan  ", "Ivan")]
    [InlineData("Anne-Marie", "Anne-Marie")]
    [InlineData("O'Brien", "O'Brien")]
    public void Create_NameProvided_IsTrimmedAndStored(string input, string expected)
    {
        var result = User.Create(input, null, null, null, null, null);

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value.Name!.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_NameBlank_TreatedAsNoName(string input)
    {
        var result = User.Create(input, null, null, null, null, null);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.Name);
    }

    [Theory]
    [InlineData("x")]            // shorter than min length
    [InlineData("Ivan123")]      // digits are not allowed
    [InlineData("Ivan_Petrov")]  // underscore is not an allowed separator
    [InlineData("!!!")]          // punctuation only
    public void Create_NameInvalid_FailsWithNameError(string input)
    {
        var result = User.Create(input, null, null, null, null, null);

        Assert.True(result.IsFailure);
        Assert.True(HasFieldError(result.Error, "Name"));
    }

    [Theory]
    [InlineData(3)]
    [InlineData(30)]
    [InlineData(99)]
    public void Create_AgeWithinRange_Succeeds(int age)
    {
        var result = User.Create(null, (short)age, null, null, null, null);

        Assert.True(result.IsSuccess);
        Assert.Equal((short)age, result.Value.Age);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(2)]
    [InlineData(100)]
    [InlineData(200)]
    public void Create_AgeOutOfRange_FailsWithAgeError(int age)
    {
        var result = User.Create(null, (short)age, null, null, null, null);

        Assert.True(result.IsFailure);
        Assert.True(HasFieldError(result.Error, "Age"));
    }

    [Theory]
    [InlineData(20)]
    [InlineData(75)]
    [InlineData(301)]
    public void Create_WeightWithinRange_Succeeds(double weight)
    {
        var result = User.Create(null, null, (decimal)weight, null, null, null);

        Assert.True(result.IsSuccess);
        Assert.Equal((decimal)weight, result.Value.BodyWeightKg);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(19.99)]
    [InlineData(302)]
    public void Create_WeightOutOfRange_FailsWithWeightError(double weight)
    {
        var result = User.Create(null, null, (decimal)weight, null, null, null);

        Assert.True(result.IsFailure);
        Assert.True(HasFieldError(result.Error, "BodyWeightKg"));
    }

    [Fact]
    public void Create_ExperienceNotDefinedInEnum_FailsWithExperienceError()
    {
        var result = User.Create(null, null, null, (ExperienceLevel)99, null, null);

        Assert.True(result.IsFailure);
        Assert.True(HasFieldError(result.Error, "Experience"));
    }

    [Fact]
    public void Create_GoalNotDefinedInEnum_FailsWithGoalError()
    {
        var result = User.Create(null, null, null, null, (TrainingGoal)99, null);

        Assert.True(result.IsFailure);
        Assert.True(HasFieldError(result.Error, "Goal"));
    }

    [Fact]
    public void Create_HealthNotesProvided_IsTrimmedAndStored()
    {
        var result = User.Create(null, null, null, null, null, "  Bad knee  ");

        Assert.True(result.IsSuccess);
        Assert.Equal("Bad knee", result.Value.HealthNotes);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_HealthNotesBlank_StoredAsNull(string input)
    {
        var result = User.Create(null, null, null, null, null, input);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.HealthNotes);
    }

    [Fact]
    public void Create_HealthNotesTooShort_FailsWithHealthNotesError()
    {
        var result = User.Create(null, null, null, null, null, "ab");

        Assert.True(result.IsFailure);
        Assert.True(HasFieldError(result.Error, "HealthNotes"));
    }

    [Fact]
    public void Create_HealthNotesTooLong_FailsWithHealthNotesError()
    {
        var result = User.Create(null, null, null, null, null, new string('a', 1000));

        Assert.True(result.IsFailure);
        Assert.True(HasFieldError(result.Error, "HealthNotes"));
    }

    [Fact]
    public void Create_SeveralNonNameFieldsInvalid_AggregatesAllErrors()
    {
        var result = User.Create(null, 200, 5m, (ExperienceLevel)99, (TrainingGoal)99, "ab");

        Assert.True(result.IsFailure);
        Assert.True(HasFieldError(result.Error, "Age"));
        Assert.True(HasFieldError(result.Error, "BodyWeightKg"));
        Assert.True(HasFieldError(result.Error, "Experience"));
        Assert.True(HasFieldError(result.Error, "Goal"));
        Assert.True(HasFieldError(result.Error, "HealthNotes"));
    }

    [Fact]
    public void Create_NameInvalidAndOtherFieldsInvalid_ReturnsOnlyNameError()
    {
        // Name is parsed first and short-circuits before the other fields are validated.
        var result = User.Create("x", 200, 5m, null, null, null);

        Assert.True(result.IsFailure);
        Assert.All(result.Error, e => Assert.Equal("Name", e.ErrorMessage.InvalidField));
    }

    // ---------------------------------------------------------------------
    // ChangeName
    // ---------------------------------------------------------------------

    [Theory]
    [InlineData("  Ivan  ", "Ivan")]
    [InlineData("Jean-Pierre", "Jean-Pierre")]
    public void ChangeName_ValidValue_NormalizesStoresAndTouches(string input, string expected)
    {
        var user = EmptyUser();

        var result = user.ChangeName(input);

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, user.Name!.Name);
        Assert.NotNull(user.UpdatedAt);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ChangeName_Blank_ClearsName(string? input)
    {
        var user = EmptyUser();
        user.ChangeName("Ivan");

        var result = user.ChangeName(input);

        Assert.True(result.IsSuccess);
        Assert.Null(user.Name);
    }

    [Theory]
    [InlineData("x")]
    [InlineData("Ivan123")]
    public void ChangeName_Invalid_FailsAndDoesNotMutate(string input)
    {
        var user = EmptyUser();
        user.ChangeName("Ivan");
        var updatedAtBefore = user.UpdatedAt;

        var result = user.ChangeName(input);

        Assert.True(result.IsFailure);
        Assert.True(HasFieldError(result.Error, "Name"));
        Assert.Equal("Ivan", user.Name!.Name);
        Assert.Equal(updatedAtBefore, user.UpdatedAt);
    }

    // ---------------------------------------------------------------------
    // ChangeAge
    // ---------------------------------------------------------------------

    [Theory]
    [InlineData(3)]
    [InlineData(45)]
    [InlineData(99)]
    public void ChangeAge_WithinRange_SetsAndTouches(int age)
    {
        var user = EmptyUser();

        var result = user.ChangeAge((short)age);

        Assert.True(result.IsSuccess);
        Assert.Equal((short)age, user.Age);
        Assert.NotNull(user.UpdatedAt);
    }

    [Fact]
    public void ChangeAge_Null_ClearsValue()
    {
        var user = EmptyUser();
        user.ChangeAge(30);

        var result = user.ChangeAge(null);

        Assert.True(result.IsSuccess);
        Assert.Null(user.Age);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(100)]
    public void ChangeAge_OutOfRange_FailsAndDoesNotMutate(int age)
    {
        var user = EmptyUser();
        user.ChangeAge(30);
        var updatedAtBefore = user.UpdatedAt;

        var result = user.ChangeAge((short)age);

        Assert.True(result.IsFailure);
        Assert.True(HasFieldError(result.Error, "Age"));
        Assert.Equal((short)30, user.Age);
        Assert.Equal(updatedAtBefore, user.UpdatedAt);
    }

    // ---------------------------------------------------------------------
    // ChangeBodyWeight
    // ---------------------------------------------------------------------

    [Theory]
    [InlineData(20)]
    [InlineData(80)]
    [InlineData(301)]
    public void ChangeBodyWeight_WithinRange_SetsAndTouches(double weight)
    {
        var user = EmptyUser();

        var result = user.ChangeBodyWeight((decimal)weight);

        Assert.True(result.IsSuccess);
        Assert.Equal((decimal)weight, user.BodyWeightKg);
        Assert.NotNull(user.UpdatedAt);
    }

    [Fact]
    public void ChangeBodyWeight_Null_ClearsValue()
    {
        var user = EmptyUser();
        user.ChangeBodyWeight(70m);

        var result = user.ChangeBodyWeight(null);

        Assert.True(result.IsSuccess);
        Assert.Null(user.BodyWeightKg);
    }

    [Theory]
    [InlineData(19.99)]
    [InlineData(302)]
    public void ChangeBodyWeight_OutOfRange_FailsAndDoesNotMutate(double weight)
    {
        var user = EmptyUser();

        var result = user.ChangeBodyWeight((decimal)weight);

        Assert.True(result.IsFailure);
        Assert.True(HasFieldError(result.Error, "BodyWeightKg"));
        Assert.Null(user.BodyWeightKg);
    }

    // ---------------------------------------------------------------------
    // ChangeExperience / ChangeGoal
    // ---------------------------------------------------------------------

    [Fact]
    public void ChangeExperience_DefinedValue_SetsAndTouches()
    {
        var user = EmptyUser();

        var result = user.ChangeExperience(ExperienceLevel.Advanced);

        Assert.True(result.IsSuccess);
        Assert.Equal(ExperienceLevel.Advanced, user.Experience);
        Assert.NotNull(user.UpdatedAt);
    }

    [Fact]
    public void ChangeExperience_Null_ClearsValue()
    {
        var user = EmptyUser();
        user.ChangeExperience(ExperienceLevel.Beginner);

        var result = user.ChangeExperience(null);

        Assert.True(result.IsSuccess);
        Assert.Null(user.Experience);
    }

    [Fact]
    public void ChangeExperience_UndefinedValue_FailsAndDoesNotMutate()
    {
        var user = EmptyUser();

        var result = user.ChangeExperience((ExperienceLevel)99);

        Assert.True(result.IsFailure);
        Assert.True(HasFieldError(result.Error, "Experience"));
        Assert.Null(user.Experience);
    }

    [Fact]
    public void ChangeGoal_DefinedValue_SetsAndTouches()
    {
        var user = EmptyUser();

        var result = user.ChangeGoal(TrainingGoal.WeightLoss);

        Assert.True(result.IsSuccess);
        Assert.Equal(TrainingGoal.WeightLoss, user.Goal);
        Assert.NotNull(user.UpdatedAt);
    }

    [Fact]
    public void ChangeGoal_Null_ClearsValue()
    {
        var user = EmptyUser();
        user.ChangeGoal(TrainingGoal.Endurance);

        var result = user.ChangeGoal(null);

        Assert.True(result.IsSuccess);
        Assert.Null(user.Goal);
    }

    [Fact]
    public void ChangeGoal_UndefinedValue_FailsAndDoesNotMutate()
    {
        var user = EmptyUser();

        var result = user.ChangeGoal((TrainingGoal)99);

        Assert.True(result.IsFailure);
        Assert.True(HasFieldError(result.Error, "Goal"));
        Assert.Null(user.Goal);
    }

    // ---------------------------------------------------------------------
    // ChangeHealthNotes
    // ---------------------------------------------------------------------

    [Fact]
    public void ChangeHealthNotes_ValidValue_NormalizesStoresAndTouches()
    {
        var user = EmptyUser();

        var result = user.ChangeHealthNotes("  Bad knee  ");

        Assert.True(result.IsSuccess);
        Assert.Equal("Bad knee", user.HealthNotes);
        Assert.NotNull(user.UpdatedAt);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ChangeHealthNotes_Blank_ClearsValue(string? input)
    {
        var user = EmptyUser();
        user.ChangeHealthNotes("Bad knee");

        var result = user.ChangeHealthNotes(input);

        Assert.True(result.IsSuccess);
        Assert.Null(user.HealthNotes);
    }

    [Fact]
    public void ChangeHealthNotes_TooShort_FailsAndDoesNotMutate()
    {
        var user = EmptyUser();
        user.ChangeHealthNotes("Bad knee");

        var result = user.ChangeHealthNotes("ab");

        Assert.True(result.IsFailure);
        Assert.True(HasFieldError(result.Error, "HealthNotes"));
        Assert.Equal("Bad knee", user.HealthNotes);
    }

    [Fact]
    public void ChangeHealthNotes_TooLong_Fails()
    {
        var user = EmptyUser();

        var result = user.ChangeHealthNotes(new string('a', 1000));

        Assert.True(result.IsFailure);
        Assert.True(HasFieldError(result.Error, "HealthNotes"));
    }
}
