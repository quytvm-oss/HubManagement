using HubManagement.Application.Features.Identities.Tokens.RefreshToken;
using HubManagement.Application.Features.Identities.Tokens.TokenGeneration;

namespace HubManagement.Application.UnitTests.Features.Identities;

public sealed class TokenCommandValidatorTests
{
    [Theory]
    [InlineData("user@example.com", "password", true)]
    [InlineData("not-an-email", "password", false)]
    [InlineData("", "password", false)]
    [InlineData("user@example.com", "", false)]
    public async Task Generate_token_validation_returns_expected_result(
        string email,
        string password,
        bool expectedValid)
    {
        var result = await new GenerateTokenCommandValidator()
            .ValidateAsync(new GenerateTokenCommand(email, password));

        Assert.Equal(expectedValid, result.IsValid);
    }

    [Theory]
    [InlineData("refresh-token", true)]
    [InlineData("", false)]
    public async Task Refresh_token_validation_returns_expected_result(
        string refreshToken,
        bool expectedValid)
    {
        var result = await new RefreshTokenCommandValidator()
            .ValidateAsync(new RefreshTokenCommand(null, refreshToken));

        Assert.Equal(expectedValid, result.IsValid);
    }
}
