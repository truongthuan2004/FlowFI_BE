namespace FlowFi.AuthUserService.DTOs;

public sealed record RegisterRequest(
    string Email,
    string Password,
    string FullName,
    string? CurrencyCode,
    string? DeviceFingerprint,
    string? DeviceName,
    string? Platform,
    string? OsVersion,
    string? AppVersion);

public sealed record LoginRequest(
    string Email,
    string Password,
    string? DeviceFingerprint,
    string? DeviceName,
    string? Platform,
    string? OsVersion,
    string? AppVersion);

public sealed record GoogleLoginRequest(
    string IdToken,
    string? DeviceFingerprint,
    string? DeviceName,
    string? Platform,
    string? OsVersion,
    string? AppVersion);

public sealed record RefreshTokenRequest(string RefreshToken);
public sealed record LogoutRequest(string RefreshToken, string? DeviceFingerprint);
public sealed record LogoutAllRequest(Guid UserId);
public sealed record ForgotPasswordRequest(string Email, string ResetMethod, string? DeviceFingerprint);
public sealed record ResetPasswordRequest(string Email, string Token, string? OtpCode, string NewPassword);
public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);
