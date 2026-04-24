using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Localization;

namespace MBET.web.Controllers
{
    [Route("[controller]/[action]")]
    public class CultureController : Controller
    {
        public IActionResult Set(string culture, string redirectUri)
        {
            if (culture != null)
            {
                HttpContext.Response.Cookies.Append(
                    CookieRequestCultureProvider.DefaultCookieName,
                    CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                    new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
                );
            }

            // Redirect back to the page the user was on
            return LocalRedirect(redirectUri);
        }
    }
}
