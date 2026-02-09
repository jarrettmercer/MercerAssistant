using System.Security.Claims;
using MercerAssistant.Core.Entities;
using MercerAssistant.Core.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace MercerAssistant.Infrastructure.Services;

public class AppUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public AppUserClaimsPrincipalFactory(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<IdentityOptions> options)
        : base(userManager, roleManager, options)
    {
        _userManager = userManager;
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);

        // Auto-assign "User" role if user has no roles
        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Count == 0)
        {
            await _userManager.AddToRoleAsync(user, "User");
            identity.AddClaim(new Claim(ClaimTypes.Role, "User"));
        }

        // Admins get all permissions automatically
        if (roles.Contains("Admin"))
        {
            foreach (var perm in AppPermission.All)
                identity.AddClaim(new Claim(AppPermission.ClaimType, perm.Value));
        }
        else
        {
            // For non-admins, load stored permission claims
            var existingClaims = await _userManager.GetClaimsAsync(user);
            var permissionClaims = existingClaims
                .Where(c => c.Type == AppPermission.ClaimType)
                .ToList();

            // If no permissions set yet, grant defaults and persist them
            if (permissionClaims.Count == 0)
            {
                var defaultClaims = AppPermission.Defaults
                    .Select(p => new Claim(AppPermission.ClaimType, p))
                    .ToList();
                await _userManager.AddClaimsAsync(user, defaultClaims);
                foreach (var c in defaultClaims)
                    identity.AddClaim(c);
            }
        }

        if (!string.IsNullOrEmpty(user.DisplayName))
            identity.AddClaim(new Claim("DisplayName", user.DisplayName));

        return identity;
    }
}
