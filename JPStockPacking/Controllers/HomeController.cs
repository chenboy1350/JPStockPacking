using JPStockPacking.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace JPStockPacking.Controllers
{
    public class HomeController(IOrderManagementService orderManagementService) : Controller
    {
        private readonly IOrderManagementService _orderManagementService = orderManagementService;

        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult OrderManagement()
        {
            _orderManagementService.ImportOrder();
            var orders = _orderManagementService.GetOrderAndLot();
            return PartialView("~/Views/Partial/_OrderManagement.cshtml", orders);
        }
    }
}
