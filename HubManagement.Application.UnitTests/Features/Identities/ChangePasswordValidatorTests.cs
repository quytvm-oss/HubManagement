using HubManagement.Application.Features.Identities.Users.ChangePassword;

namespace HubManagement.Application.UnitTests.Features.Identities;

public sealed class ChangePasswordValidatorTests
{
    private readonly ChangePasswordValidator _validator = new();

    [Fact]
    public async Task Validate_matching_new_password_succeeds()
    {
        var command = new ChangePasswordCommand
        {
            Password = "old-password",
            NewPassword = "new-password",
            ConfirmNewPassword = "new-password"
        };

        var result = await _validator.ValidateAsync(command);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("", "new-password", "new-password", nameof(ChangePasswordCommand.Password))]
    [InlineData("same-password", "same-password", "same-password", nameof(ChangePasswordCommand.NewPassword))]
    [InlineData("old-password", "new-password", "different", nameof(ChangePasswordCommand.ConfirmNewPassword))]
    public async Task Validate_invalid_password_change_reports_expected_property(
        string currentPassword,
        string newPassword,
        string confirmation,
        string expectedProperty)
    {
        var command = new ChangePasswordCommand
        {
            Password = currentPassword,
            NewPassword = newPassword,
            ConfirmNewPassword = confirmation
        };

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == expectedProperty);
    }
}
