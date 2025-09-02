using JPStockPacking.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static JPStockPacking.Services.Helper.Enum;

namespace JPStockPacking.Controllers
{
    public class HomeController(IOrderManagementService orderManagementService, INotificationService notificationService) : Controller
    {
        private readonly IOrderManagementService _orderManagementService = orderManagementService;
        private readonly INotificationService _notificationService = notificationService;

        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> OrderManagement()
        {
            //await _orderManagementService.ImportOrderAsync();
            await _orderManagementService.GetUpdateLotAsync();
            ViewBag.Tables = await _orderManagementService.GetTableAsync();
            var result = await _orderManagementService.GetOrderAndLotByRangeAsync(GroupMode.Day, string.Empty, string.Empty, DateTime.MinValue, DateTime.MinValue);
            return PartialView("~/Views/Partial/_OrderManagement.cshtml", result);
        }

        [Authorize]
        public IActionResult DashBoard()
        {
            return PartialView("~/Views/Partial/_DashBoardPartial.cshtml");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ImportOrder(string orderNo)
        {
            if (orderNo == string.Empty && orderNo == null) return BadRequest();
            await _orderManagementService.ImportOrderAsync(orderNo);
            await _orderManagementService.GetUpdateLotAsync();
            return Ok();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetOrder(string orderNo, string custCode, DateTime fdate, DateTime edate, GroupMode groupMode)
        {
            var result = await _orderManagementService.GetOrderAndLotByRangeAsync(groupMode, orderNo, custCode, fdate, edate);
            return Ok(result);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetReceived(string lotNo)
        {
            return Ok(await _orderManagementService.GetReceivedAsync(lotNo));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetReceivedToAssign(string lotNo)
        {
            return Ok(await _orderManagementService.GetReceivedToAssignAsync(lotNo));
        }

        [HttpPatch]
        [Authorize]
        public async Task<IActionResult> UpdateLotItems([FromForm] string lotNo, [FromForm] int[] receivedIDs)
        {
            if (string.IsNullOrWhiteSpace(lotNo) || receivedIDs == null || receivedIDs.Length == 0)
                return BadRequest("ข้อมูลไม่ครบถ้วน");

            await _orderManagementService.UpdateReceivedItemsAsync(lotNo, receivedIDs);

            var updatedLot = await _orderManagementService.GetCustomLotAsync(lotNo);

            if (updatedLot == null)
                return NotFound();

            return Ok(updatedLot);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AssignToTable([FromForm] string lotNo, [FromForm] int[] receivedIDs, [FromForm] string tableId, [FromForm] string[] memberIds)
        {
            if (string.IsNullOrWhiteSpace(lotNo) || receivedIDs == null || receivedIDs.Length == 0 || string.IsNullOrWhiteSpace(tableId) || memberIds == null || memberIds.Length == 0)
                return BadRequest("ข้อมูลไม่ครบถ้วน");
            await _orderManagementService.AssignReceivedAsync(lotNo, receivedIDs, tableId, memberIds);
            return Ok();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetTableMember(int tableID)
        {
            var result = await _orderManagementService.GetTableMemberAsync(tableID);
            return Ok(result);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetTableToReturn(string LotNo)
        {
            var result = await _orderManagementService.GetTableToReturnAsync(LotNo);
            return Ok(result);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetRecievedToReturn(string LotNo, int TableID)
        {
            var result = await _orderManagementService.GetRecievedToReturnAsync(LotNo, TableID);
            return Ok(result);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ReturnAssignment([FromForm] string lotNo, [FromForm] int[] assignmentIDs, [FromForm] decimal lostQty, [FromForm] decimal breakQty, [FromForm] decimal returnQty)
        {

            await _orderManagementService.ReturnReceivedAsync(lotNo, assignmentIDs, lostQty, breakQty, returnQty);
            return Ok();
        }

        [HttpPatch]
        [Authorize]
        public async Task<IActionResult> LostAndRepair([FromForm] string lotNo, [FromForm] int[] assignmentIDs, [FromForm] decimal lostQty, [FromForm] decimal breakQty, [FromForm] decimal returnQty)
        {
            await _orderManagementService.LostAndRepairAsync(lotNo, assignmentIDs, lostQty, breakQty, returnQty);
            return Ok();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> OrderMarkAsRead(string orderNo)
        {
            if (string.IsNullOrWhiteSpace(orderNo)) return BadRequest();
            await _notificationService.OrderMarkAsReadAsync(orderNo);
            return Ok();
        }
    }
}
