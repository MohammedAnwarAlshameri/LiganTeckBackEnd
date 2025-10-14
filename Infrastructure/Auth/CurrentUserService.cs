using System.Security.Claims;
using Application.Common;

using Microsoft.AspNetCore.Http;

namespace Infrastructure.Auth;

public sealed class CurrentUserService : ICurrentUser
{
    private readonly IHttpContextAccessor _http;

    public CurrentUserService(IHttpContextAccessor http) => _http = http;

    public bool IsAuthenticated => _http.HttpContext?.User?.Identity?.IsAuthenticated == true;

    public long? TenantId
    {
        get
        {
            var user = _http.HttpContext?.User;
            if (user == null) return null;

            // نحاول بالأولوية من claim "tenant_id" التي نولّدها في JwtTokenService
            var idStr = user.FindFirstValue("tenant_id")
                      ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (long.TryParse(idStr, out var id)) return id;
            return null;
        }
    }

    public string? Email => _http.HttpContext?.User?.FindFirstValue(ClaimTypes.Email)
                          ?? _http.HttpContext?.User?.FindFirstValue("email");

    public string? Name => _http.HttpContext?.User?.FindFirstValue(ClaimTypes.Name);
}