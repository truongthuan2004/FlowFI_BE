namespace FlowFi.AuthUserService.DTOs;

public sealed record RegisterRequest(
    string Email,
    string Password,
    string FullName);

public sealed record LoginRequest(
    string Email,
    string Password);

public sealed record GoogleLoginRequest(string IdToken);

public sealed record RefreshTokenRequest(string RefreshToken);

public sealed record LogoutRequest(string RefreshToken);

public sealed record ForgotPasswordRequest(string Email);

public sealed record ResetPasswordRequest(string Email, string Token, string NewPassword);

public sealed record ChangePasswordRequest(
    Guid UserId,
    string CurrentPassword,
    string NewPassword);

public sealed record UpdateProfileRequest(
    string? FullName,
    string? AvatarUrl,
    DateOnly? DateOfBirth);

public sealed record UpdatePreferencesRequest(
    string? CurrencyCode,
    decimal? MonthlyBudgetLimit);
