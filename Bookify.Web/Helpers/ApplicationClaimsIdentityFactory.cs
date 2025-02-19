using Microsoft.Extensions.Options;

namespace Bookify.Web.Helpers
{
    public class ApplicationClaimsIdentityFactory : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
    {
        public ApplicationClaimsIdentityFactory(UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager, IOptions<IdentityOptions> options) : base(userManager, roleManager, options)
        {
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
        {
            var principal = await base.GenerateClaimsAsync(user);

            principal.AddClaim(new Claim(ClaimTypes.GivenName, user.FullName));

            return principal;
        }
    }
}