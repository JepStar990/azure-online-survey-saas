using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace SurveyApi.Infrastructure.Auth;

/// <summary>
/// Extracts current user information from the HTTP context's authenticated claims principal.
/// Provides the user's Azure AD object ID, display name, and tenant ID for authorization decisions.
/// </summary>
public class CurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// The Azure AD object identifier (oid claim) of the authenticated user.
    /// Returns null if the user is not authenticated.
    /// </summary>
    public Guid? UserId
    {
        get
        {
            var oid = _httpContextAccessor.HttpContext?.User.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier")
                      ?? _httpContextAccessor.HttpContext?.User.FindFirstValue("oid");
            return oid is not null && Guid.TryParse(oid, out var guid) ? guid : null;
        }
    }

    /// <summary>
    /// The display name of the authenticated user.
    /// </summary>
    public string? DisplayName =>
        _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Name)
        ?? _httpContextAccessor.HttpContext?.User.FindFirstValue("name");

    /// <summary>
    /// The email address of the authenticated user.
    /// </summary>
    public string? Email =>
        _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Email)
        ?? _httpContextAccessor.HttpContext?.User.FindFirstValue("preferred_username");

    /// <summary>
    /// The Azure AD tenant ID (tid claim) of the authenticated user.
    /// </summary>
    public string? TenantId =>
        _httpContextAccessor.HttpContext?.User.FindFirstValue("http://schemas.microsoft.com/identity/claims/tenantid")
        ?? _httpContextAccessor.HttpContext?.User.FindFirstValue("tid");

    /// <summary>
    /// Whether the current request has an authenticated user.
    /// </summary>
    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;

    /// <summary>
    /// The roles assigned to the current user from the 'roles' claim.
    /// </summary>
    public IEnumerable<string> Roles =>
        _httpContextAccessor.HttpContext?.User.FindAll(ClaimTypes.Role).Select(c => c.Value)
        ?? _httpContextAccessor.HttpContext?.User.FindAll("roles").Select(c => c.Value)
        ?? Array.Empty<string>();
}
