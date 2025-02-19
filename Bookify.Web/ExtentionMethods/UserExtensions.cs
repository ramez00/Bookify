namespace Bookify.Web.ExtentionMethods
{
    public static class UserExtensions
    {
        public static string GetUserId(this ClaimsPrincipal claims)
            => claims.FindFirst(ClaimTypes.NameIdentifier)!.Value;
    }
}
