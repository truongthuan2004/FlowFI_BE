using System.Security.Claims;

namespace FlowFi.Common.Authentication;

public interface ITokenService
{
    string CreateAccessToken(IEnumerable<Claim> claims);
    string CreateRefreshToken();
}
