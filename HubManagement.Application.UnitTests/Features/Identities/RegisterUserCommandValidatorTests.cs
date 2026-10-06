using HubManagement.Application.Features.Identities.Users.RegisterUser;

namespace HubManagement.Application.UnitTests.Features.Identities;

public sealed class RegisterUserCommandValidatorTests
{
    private readonly RegisterUserCommandValidator _validator = new();

    [Fact]
    public async Task Validate_valid_command_succeeds()
    {
        var result = await _validator.ValidateAsync(CreateValidCommand());

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("", "Doe", "john@example.com", "john", "secret1", "secret1", nameof(RegisterUserCommand.FirstName))]
    [InlineData("John", "", "john@example.com", "john", "secret1", "secret1", nameof(RegisterUserCommand.LastName))]
    [InlineData("John", "Doe", "invalid", "john", "secret1", "secret1", nameof(RegisterUserCommand.Email))]
    [InlineData("John", "Doe", "john@example.com", "ab", "secret1", "secret1", nameof(RegisterUserCommand.UserName))]
    [InlineData("John", "Doe", "john@example.com", "john", "123", "123", nameof(RegisterUserCommand.Password))]
    [InlineData("John", "Doe", "john@example.com", "john", "secret1", "different", nameof(RegisterUserCommand.ConfirmPassword))]
    public async Task Validate_invalid_command_reports_expected_property(
        string firstName,
        string lastName,
        string email,
        string userName,
        string password,
        string confirmation,
        string expectedProperty)
    {
        var command = new RegisterUserCommand
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            UserName = userName,
            Password = password,
            ConfirmPassword = confirmation
        };

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == expectedProperty);
    }

    private static RegisterUserCommand CreateValidCommand() => new()
    {
        FirstName = "John",
        LastName = "Doe",
        Email = "john@example.com",
        UserName = "john.doe",
        Password = "secret1",
        ConfirmPassword = "secret1",
        PhoneNumber = "+84901234567"
    };
}
