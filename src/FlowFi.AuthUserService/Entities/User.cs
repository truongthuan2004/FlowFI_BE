namespace FlowFi.AuthUserService.Entities;

public sealed class User
{
    public Guid Id { get; set; }

    public string Email { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }

    public string? FullName { get; set; }
    public string? AvatarUrl { get; set; }
    public DateOnly? DateOfBirth { get; set; }

    public string CurrencyCode { get; set; } = "VND";
    public decimal? MonthlyBudgetLimit { get; set; }

    public string AuthProvider { get; set; } = "LOCAL";
    public bool IsVerified { get; set; }
    public string Role { get; set; } = UserRoles.User;

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
