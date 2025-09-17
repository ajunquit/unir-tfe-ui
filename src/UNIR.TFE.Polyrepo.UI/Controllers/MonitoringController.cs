using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UNIR.TFE.Polyrepo.UI.Controllers
{
    public class MonitoringController : Controller
    {
        [AllowAnonymous]
        [HttpGet("/health")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Health() => Content("OK", "text/plain");
    }
}
