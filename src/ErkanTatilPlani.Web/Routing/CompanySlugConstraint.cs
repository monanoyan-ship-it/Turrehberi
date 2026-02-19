namespace ErkanTatilPlani.Web.Routing;

public class CompanySlugConstraint : IRouteConstraint
{
    private static readonly HashSet<string> ReservedSlugs = new(StringComparer.OrdinalIgnoreCase)
    {
        "Home", "Admin", "Account", "MyCompany", "Companies", "Blog",
        "Tours", "Reservations", "Visitors", "sitemap.xml",
        "lib", "js", "css", "images", "favicon.ico"
    };

    public bool Match(HttpContext? httpContext, IRouter? route, string routeKey,
        RouteValueDictionary values, RouteDirection routeDirection)
    {
        if (values.TryGetValue(routeKey, out var value) && value is string slug)
        {
            return !string.IsNullOrEmpty(slug) && !ReservedSlugs.Contains(slug);
        }
        return false;
    }
}
