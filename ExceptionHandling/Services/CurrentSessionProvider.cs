using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ExceptionHandling.Services;

public interface ICurrentSessionProvider
{
    Guid? GetUserId();
}

public class CurrentSessionProvider : ICurrentSessionProvider
{
    private readonly Guid? _currentUserId;

    public CurrentSessionProvider(IHttpContextAccessor accessor)
    {
        //var userId = accessor.HttpContext?.User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        var identity = accessor.HttpContext?.User.Identity as ClaimsIdentity;
        if(identity?.IsAuthenticated != true)
        {
            return;
        }

        var userId = identity.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId is null)
        {
            return;
        }

        _currentUserId = Guid.TryParse(userId, out var guid) ? guid : null;
    }

    public Guid? GetUserId() => _currentUserId;
}

