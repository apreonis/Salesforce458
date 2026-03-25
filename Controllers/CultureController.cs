using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.Controllers;

[Route("culture")]
public class CultureController : Controller
{
    [HttpGet("set")]
    public IActionResult Set(string culture, string redirectUri = "/")
    {
        var selectedCulture = string.Equals(culture, "ru", StringComparison.OrdinalIgnoreCase) ? "ru" : "en";

        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(selectedCulture, selectedCulture)),
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true,
                SameSite = SameSiteMode.Lax,
                Path = "/"
            });

        if (string.IsNullOrWhiteSpace(redirectUri))
            redirectUri = "/";

        return LocalRedirect(redirectUri);
    }
}