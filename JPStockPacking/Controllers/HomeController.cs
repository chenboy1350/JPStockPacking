using JPStockPacking.Models;
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
            ViewBag.Username = User.Identity?.Name!.ToLower();
            return View();
        }

        [Authorize]
        public async Task<IActionResult> OrderManagement()
        {
            //await _orderManagementService.ImportOrderAsync();
            //await _orderManagementService.GetUpdateLotAsync();
            ViewBag.Tables = await _orderManagementService.GetTableAsync();
            var result = await _orderManagementService.GetOrderAndLotByRangeAsync(GroupMode.Day, string.Empty, string.Empty, DateTime.MinValue, DateTime.MinValue);
            return PartialView("~/Views/Partial/_OrderManagement.cshtml", result);
        }

        [Authorize]
        public IActionResult DashBoard()
        {
            return PartialView("~/Views/Partial/_DashBoardPartial.cshtml");
        }

        [Authorize]
        public IActionResult RecievedManagement()
        {
            return PartialView("~/Views/Partial/_RecievedManagement.cshtml");
        }

        [Authorize]
        public IActionResult CheckQtyToPack()
        {
            return PartialView("~/Views/Partial/_CheckQtyToPack.cshtml");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ImportOrder(string orderNo)
        {
            if (orderNo == string.Empty && orderNo == null) return BadRequest();
            await _orderManagementService.ImportOrderAsync(orderNo);
            //await _orderManagementService.GetUpdateLotAsync();
            return Ok();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ImportReceiveNo(string receiveNo)
        {
            if (receiveNo == string.Empty && receiveNo == null) return BadRequest();
            var res = await _orderManagementService.GetJPReceivedByReceiveNoAsync(receiveNo);
            return Ok(res);
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

        [HttpPatch]
        [Authorize]
        public async Task<IActionResult> UpdateLotByRevNoItems([FromForm] string receiveNo)
        {
            if (string.IsNullOrWhiteSpace(receiveNo)) return BadRequest("ข้อมูลไม่ครบถ้วน");

            await _orderManagementService.UpdateAllReceivedItemsAsync(receiveNo);

            return Ok();
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
        public async Task<IActionResult> GetOrderToSendQty(string orderNo)
        {
            var result = await _orderManagementService.GetOrderToSendQtyAsync(orderNo);
            return Ok(result);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DefineToPack([FromForm] DefineToPackRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.OrderNo) || request.Lots == null || request.Lots.Count == 0)
                return BadRequest("ข้อมูลไม่ครบถ้วน");

            try
            {
                await _orderManagementService.DefineToPackAsync(request.OrderNo, request.Lots);
                return Ok("บันทึกข้อมูลเรียบร้อย");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            { 
                return StatusCode(500, "เกิดข้อผิดพลาดภายในระบบ");
            }
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
