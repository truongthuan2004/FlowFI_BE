namespace FlowFi.AuthUserService.Entities;

public sealed record OAuthIdentity(Guid Id, Guid UserId, string Provider, string ProviderUserId);

