using Bookify.Domain.Models;
using Bookify.Infrastructure;
using Bookify.Web;
using Bookify.Web.ApplicationTasks;
using Bookify.Web.Seeds;
using Bookify.Web.Services;
using Hangfire;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Identity.UI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddWebServices(builder);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();


app.UseCookiePolicy(new CookiePolicyOptions
{
    Secure = CookieSecurePolicy.Always
});

app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Frame-Options", "Deny");
    await next();
});

app.UseAuthentication();
app.UseAuthorization();

var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();

using var scope = scopeFactory.CreateScope();

var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
var userManger = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();


await DefaultUsers.SeedAdminUser(userManger);
await DefaultRoles.SeedRoles(roleManager);

app.UseHangfireDashboard("/AppTasks", new DashboardOptions
{
    DashboardTitle = "Clinify Background Tasks",
    // IsReadOnlyFunc = (DashboardContext context) => true,
    Authorization = new IDashboardAuthorizationFilter[]
    {
       new HangfireAuthorizationFilter("AdminOnly")
    }
});

var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
var emailBodyBuilder = scope.ServiceProvider.GetRequiredService<IEmailBodyBuilder>();
var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

var BackgroundTasks = new HangFireSendTasks(context, emailBodyBuilder, emailSender);

RecurringJob.AddOrUpdate(() => BackgroundTasks.PrepareSubscriberAlert(), cronExpression: "0 14 * * *");
RecurringJob.AddOrUpdate(() => BackgroundTasks.PrepareSubscriberRentalExpirationAlert(), cronExpression: "0 15 * * *");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
