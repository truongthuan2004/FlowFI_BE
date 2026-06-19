namespace FlowFi.AuthUserService.DTOs;

public sealed record UpdateProfileRequest(
    string? FullName,
    string? AvatarUrl,
    DateOnly? DateOfBirth);

public sealed record UpdatePreferencesRequest(
    string? CurrencyCode,
    decimal? MonthlyBudgetLimit);
