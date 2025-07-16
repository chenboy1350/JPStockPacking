using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JPStockPacking.Controllers
{
    public class HomeController : Controller
    {
        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult OrderManagement()
        {
            return PartialView("~/Views/Partial/_OrderManagement.cshtml");
        }
    }
}
