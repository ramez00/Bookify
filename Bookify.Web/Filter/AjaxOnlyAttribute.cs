using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;

namespace Bookify.Web.Filter
{
    public class AjaxOnlyAttribute : ActionMethodSelectorAttribute
    {
        public override bool IsValidForRequest(RouteContext routeContext, ActionDescriptor action)
        {
            var req = routeContext.HttpContext.Request;
            var isValid = req.Headers["X-Requested-With"] == "XMLHttpRequest";
            return isValid;
        }
    }
}
