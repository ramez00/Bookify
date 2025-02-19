using Hangfire.Annotations;
using Hangfire.Dashboard;

namespace Bookify.Web.Filter
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        private readonly string _policyName;

        public HangfireAuthorizationFilter(string policyName)
        {
            _policyName = policyName;
        }
        public bool Authorize([NotNull] DashboardContext context)
        {
            var httpContext = context.GetHttpContext();

            var authorizedServiec = httpContext.RequestServices.GetRequiredService<IAuthorizationService>();

            return authorizedServiec.AuthorizeAsync(httpContext.User, _policyName)
                                    .ConfigureAwait(false)
                                    .GetAwaiter()
                                    .GetResult()
                                    .Succeeded;
        }
    }
}
