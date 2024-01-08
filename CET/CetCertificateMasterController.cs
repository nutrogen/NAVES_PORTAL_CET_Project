using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using NavesPortalforWebWithCoreMvc.Controllers.AuthFromIntranetController;

namespace NavesPortalforWebWithCoreMvc.Controllers.CET
{
    [Authorize]
    [CheckSession]
    public class CetCertificateMasterController : Controller

    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
