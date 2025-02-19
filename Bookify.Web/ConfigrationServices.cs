using Bookify.Web.Cloudinary;
using Bookify.Web.Core.Mapping;
using Bookify.Web.Helpers;
using Bookify.Web.Services;
using Bookify.Web.Settings;
using Hangfire;
using HashidsNet;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Reflection;
using UoN.ExpressiveAnnotations.NetCore.DependencyInjection;
using ViewToHTML.Extensions;
using WhatsAppCloudApi.Extensions;

namespace Bookify.Web
{
    public static class ConfigrationServices
    {
        public static IServiceCollection AddWebServices(this IServiceCollection services,
            WebApplicationBuilder builder)
        {
            // register ViewToHTML service
            services.AddViewToHTML();

            // any action related to User profile should br apply once Save it 

            services.Configure<SecurityStampValidatorOptions>(options => options.ValidationInterval = TimeSpan.Zero);

            // Add Custom Claim to Standard Claim

            services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, ApplicationClaimsIdentityFactory>();

            //services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
            //    .AddEntityFrameworkStores<ApplicationDbContext>();

            // Register for User And Role Identity in ur Application 

            services.AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultUI()                                  //   ===> To show Pages related to rigister and login
                .AddDefaultTokenProviders();                     //   ===>  To use Forget Password


            // Password Configration 

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1);
                options.Lockout.MaxFailedAccessAttempts = 3;
            });


            services.AddSingleton<IHashids>(_ => new Hashids("RAdi@123RmZ", minHashLength: 15));

            // add Data Protection 
            services.AddDataProtection();

            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddControllersWithViews();

            services.AddAutoMapper(Assembly.GetAssembly(typeof(MappingProfile)));

            services.AddTransient<IImageService, ImageService>();

            services.AddTransient<IEmailSender, EmailSender>();
            services.AddTransient<IEmailBodyBuilder, EmailBodyBuilder>();
            services.AddTransient<IWhatsAppService, WhatsAppService>();

            services.Configure<CloundinarySettings>(builder.Configuration.GetSection(nameof(CloundinarySettings)));

            services.Configure<MailSettings>(builder.Configuration.GetSection(nameof(MailSettings)));

            services.AddWhatsAppApiClient(builder.Configuration);

            services.AddExpressiveAnnotations();

            services.AddHangfire(x => x.UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));
            services.AddHangfireServer();

            // to Make Hangfire Dashboard OPenned at Auhonticated User only and with Admin Role Assigned

            services.Configure<AuthorizationOptions>(opt =>
            opt.AddPolicy("AdminOnly", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole(ApplicationRoles.Admin);
            }));



            return services;
        }
    }
}
