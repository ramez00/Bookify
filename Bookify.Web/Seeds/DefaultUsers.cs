namespace Bookify.Web.Seeds
{
    public static class DefaultUsers
    {
        public static async Task SeedAdminUser(UserManager<ApplicationUser> UserManager)
        {
            ApplicationUser admin = new()
            {
                UserName = "admin",
                FullName = "Admin",
                Email = "admin@Clinify.com",
                EmailConfirmed = true,
            };

            var user = await UserManager.FindByEmailAsync(admin.Email)
                ?? await UserManager.FindByNameAsync(admin.UserName);

            if (user is null)
            {
                var result = await UserManager.CreateAsync(admin, "Passw@123");

                if (result.Succeeded)
                    await UserManager.AddToRoleAsync(admin, ApplicationRoles.Admin);
            }
        }
    }
}
