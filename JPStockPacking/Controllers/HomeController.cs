using Azure.Core;
using JPStockPacking.Models;
using JPStockPacking.Services.Implement;
using JPStockPacking.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using static JPStockPacking.Services.Helper.Enum;

namespace JPStockPacking.Controllers
{
    public class HomeController(IOrderManagementService orderManagementService,
        INotificationService notificationService,
        IReportService reportService,
        IReceiveManagementService receiveManagementService,
        IWebHostEnvironment webHostEnvironment) : Controller
    {
        private readonly IOrderManagementService _orderManagementService = orderManagementService;
        private readonly INotificationService _notificationService = notificationService;
        private readonly IReportService _reportService = reportService;
        private readonly IReceiveManagementService _receiveManagementService = receiveManagementService;
        private readonly IWebHostEnvironment _env = webHostEnvironment;

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
            ViewBag.BreakDescriptions = await _orderManagementService.GetBreakDescriptionsAsync();
            var result = await _orderManagementService.GetOrderAndLotByRangeAsync(GroupMode.Day, string.Empty, string.Empty, DateTime.MinValue, DateTime.MinValue);
            return PartialView("~/Views/Partial/_OrderManagement.cshtml", result);
        }

        [Authorize]
        public IActionResult DashBoard()
        {
            return PartialView("~/Views/Partial/_DashBoardPartial.cshtml");
        }

        [Authorize]
        public async Task<IActionResult> Receive()
        {
            var result = await _receiveManagementService.GetTopJPReceivedAsync(null);
            return PartialView("~/Views/Partial/_ReceiveManagment.cshtml", result);
        }

        [Authorize]
        public IActionResult Export()
        {
            return PartialView("~/Views/Partial/_ExportManagement.cshtml");
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
            try
            {
                if (orderNo == string.Empty && orderNo == null) return BadRequest();
                await _orderManagementService.ImportOrderAsync(orderNo);
                //await _orderManagementService.GetUpdateLotAsync();
                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch 
            {
                return StatusCode(500, "เกิดข้อผิดพลาดภายในระบบ");
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ImportReceiveNo(string receiveNo)
        {
            if (receiveNo == string.Empty && receiveNo == null) return BadRequest();
            var res = await _receiveManagementService.GetJPReceivedByReceiveNoAsync(receiveNo);
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
        public async Task<IActionResult> UpdateLotItems([FromForm] string receiveNo, [FromForm] string[] orderNos, [FromForm] int[] receiveIds)
        {
            if (string.IsNullOrWhiteSpace(receiveNo) || receiveIds == null || receiveIds.Length == 0 || orderNos == null || orderNos.Length == 0)
                return BadRequest("ข้อมูลไม่ครบถ้วน");

            try
            {
                await _receiveManagementService.UpdateLotItemsAsync(receiveNo, orderNos, receiveIds);
                return Ok();
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

        [HttpPatch]
        [Authorize]
        public async Task<IActionResult> UpdateLotByRevNoItems([FromForm] string receiveNo)
        {
            if (string.IsNullOrWhiteSpace(receiveNo)) return BadRequest("ข้อมูลไม่ครบถ้วน");

            try
            {
                await _orderManagementService.UpdateAllReceivedItemsAsync(receiveNo);
                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch
            {
                return StatusCode(500, "เกิดข้อผิดพลาดภายในระบบ");
            }
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
        public async Task<IActionResult> GetTableMemberByAssignedID(string assignedID)
        {
            return Ok();
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
        public async Task<IActionResult> SendQtyToPackReport(string orderNo, PrintTo printTo)
        {
            var result = await _orderManagementService.GetOrderToSendQtyAsync(orderNo);
            var pdfBytes = _reportService.GenerateSendQtyToPackReport(result, printTo);

            var contentDisposition = $"inline; filename=TestReport_{DateTime.Now:yyyyMMdd}.pdf";
            Response.Headers.Append("Content-Disposition", contentDisposition);

            return File(pdfBytes, "application/pdf");
        }

        [HttpGet]
        [Authorize]
        public IActionResult GetImage(string filename)
        {
            var imgPath = Path.Combine(_env.WebRootPath, "img", "blankimg.png");

            if (string.IsNullOrEmpty(filename))
                return BadRequest("Missing filename.");

            filename = Path.GetFileName(filename);
            var fullPath = Path.Combine("\\\\factoryserver\\bmp$", filename);

            if (!System.IO.File.Exists(fullPath))
                fullPath = imgPath;

            var contentType = GetContentType(fullPath);
            if (string.IsNullOrEmpty(contentType))
                contentType = "application/octet-stream";

            var imageBytes = System.IO.File.ReadAllBytes(fullPath);
            return File(imageBytes, contentType);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetReceiveRow(string receiveNo)
        {
            var result = await _receiveManagementService.GetTopJPReceivedAsync(receiveNo);
            return Ok(result.FirstOrDefault());
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetReceiveList(string receiveNo)
        {
            var result = await _receiveManagementService.GetTopJPReceivedAsync(receiveNo);
            return Ok(result);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> OrderMarkAsRead(string orderNo)
        {
            if (string.IsNullOrWhiteSpace(orderNo)) return BadRequest();
            await _notificationService.OrderMarkAsReadAsync(orderNo);
            return Ok();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CheckUser([FromForm] string username, [FromForm] string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return BadRequest("ข้อมูลไม่ครบถ้วน");

            try
            {
                var res = await _orderManagementService.GetJPUser(username, password);
                return Ok(res);
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

        private static string? GetContentType(string path)
        {
            var extension = Path.GetExtension(path).ToLowerInvariant();

            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".webp" => "image/webp",
                _ => null
            };
        }
    }
}
