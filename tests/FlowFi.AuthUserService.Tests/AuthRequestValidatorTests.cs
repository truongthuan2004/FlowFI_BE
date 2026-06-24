using FlowFi.AuthUserService.DTOs;
using FlowFi.AuthUserService.Validators;
using Xunit;

namespace FlowFi.AuthUserService.Tests;

public sealed class AuthRequestValidatorTests
{
    [Fact]
    public void RegisterRequestValidator_rejects_invalid_email_and_short_password()
    {
        var validator = new RegisterRequestValidator();

        var result = validator.Validate(new RegisterRequest("not-email", "short", ""));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(RegisterRequest.Email));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(RegisterRequest.Password));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(RegisterRequest.FullName));
    }

    [Fact]
    public void LoginRequestValidator_requires_email_and_password()
    {
        var validator = new LoginRequestValidator();

        var result = validator.Validate(new LoginRequest("", ""));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(LoginRequest.Email));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(LoginRequest.Password));
    }

    [Fact]
    public void ResetPasswordRequestValidator_requires_email_token_and_strong_new_password()
    {
        var validator = new ResetPasswordRequestValidator();

        var result = validator.Validate(new ResetPasswordRequest("bad-email", "", "short"));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(ResetPasswordRequest.Email));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(ResetPasswordRequest.Token));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(ResetPasswordRequest.NewPassword));
    }

    [Fact]
    public void ChangePasswordRequestValidator_rejects_empty_current_password_and_short_new_password()
    {
        var validator = new ChangePasswordRequestValidator();

        var result = validator.Validate(new ChangePasswordRequest(Guid.NewGuid(), "", "short"));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(ChangePasswordRequest.CurrentPassword));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(ChangePasswordRequest.NewPassword));
    }

    [Fact]
    public void UpdatePreferencesRequestValidator_rejects_invalid_currency_and_negative_budget()
    {
        var validator = new UpdatePreferencesRequestValidator();

        var result = validator.Validate(new UpdatePreferencesRequest("VN", -1));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdatePreferencesRequest.CurrencyCode));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdatePreferencesRequest.MonthlyBudgetLimit));
    }
}
