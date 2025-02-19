namespace Bookify.Web.Seeds
{
    public static class DefaultRoles
    {
        public static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            if (!roleManager.Roles.Any())
            {
                await roleManager.CreateAsync(new IdentityRole(ApplicationRoles.Admin));
                await roleManager.CreateAsync(new IdentityRole(ApplicationRoles.Archive));
                await roleManager.CreateAsync(new IdentityRole(ApplicationRoles.Reception));
            }
        }
    }
}
