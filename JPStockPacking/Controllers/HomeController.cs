using JPStockPacking.Data.SPDbContext.Entities;
using JPStockPacking.Models;
using JPStockPacking.Services.Helper;
using JPStockPacking.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;
using static JPStockPacking.Services.Helper.Enum;

namespace JPStockPacking.Controllers
{
    public class HomeController(IOrderManagementService orderManagementService,
        INotificationService notificationService,
        IReportService reportService,
        IReceiveManagementService receiveManagementService,
        IWebHostEnvironment webHostEnvironment,
        IOptions<AppSettingModel> appSettings,
        IPISService pISService,
        IConfiguration configuration,
        IAssignmentService assignmentService,
        ICheckQtyToSendService checkQtyToSendService,
        IBreakService breakService,
        ILostService lostService,
        IPackedMangementService packedMangementService,
        IAuditService comparedInvoiceService,
        Serilog.ILogger logger,
        IProductionPlanningService productionPlanningService,
        IFormulaManagementService formulaManagementService,
        IPermissionManagement permissionManagement,
        IReturnService returnService) : Controller
    {
        private readonly IOrderManagementService _orderManagementService = orderManagementService;
        private readonly INotificationService _notificationService = notificationService;
        private readonly IReportService _reportService = reportService;
        private readonly IReceiveManagementService _receiveManagementService = receiveManagementService;
        private readonly IWebHostEnvironment _env = webHostEnvironment;
        private readonly AppSettingModel _appSettings = appSettings.Value;
        private readonly IPISService _pISService = pISService;
        private readonly IConfiguration _configuration = configuration;
        private readonly IAssignmentService _assignmentService = assignmentService;
        private readonly ICheckQtyToSendService _checkQtyToSendService = checkQtyToSendService;
        private readonly IBreakService _breakService = breakService;
        private readonly ILostService _lostService = lostService;
        private readonly IPackedMangementService _packedMangementService = packedMangementService;
        private readonly IAuditService _auditService = comparedInvoiceService;
        private readonly IProductionPlanningService _productionPlanningService = productionPlanningService;
        private readonly Serilog.ILogger _logger = logger;
        private readonly IFormulaManagementService _formulaManagementService = formulaManagementService;
        private readonly IPermissionManagement _permissionManagement = permissionManagement;
        private readonly IReturnService _returnService = returnService;

        [Authorize]
        public IActionResult Index()
        {
            ViewBag.AppVersion = _appSettings.AppVersion;
            return View();
        }

        [Authorize]
        public async Task<IActionResult> OrderManagement()
        {
            await _orderManagementService.ImportOrderAsync();
            ViewBag.Tables = await _orderManagementService.GetTableAsync();
            ViewBag.BreakDescriptions = await _breakService.GetBreakDescriptionsAsync();
            var result = await _orderManagementService.GetOrderAndLotByRangeAsync(string.Empty, string.Empty, string.Empty, DateTime.MinValue, DateTime.MinValue, 1, 10);
            return PartialView("~/Views/Partial/_OrderManagement.cshtml", result);
        }

        [Authorize]
        public async Task<IActionResult> PackingPlaning()
        {
            await _productionPlanningService.GetOperateOrderToPlan(DateTime.UtcNow, DateTime.UtcNow.AddDays(7));
            return PartialView("~/Views/Partial/_PackingPlaning.cshtml");
        }

        [Authorize]
        public async Task<IActionResult> ReceiveManagement()
        {
            var result = await _receiveManagementService.GetTopJPReceivedAsync(null, null, null);
            _logger.Information("GetTopJPReceivedAsync : {@result}", result);

            return PartialView("~/Views/Partial/_ReceiveManagment.cshtml", result);
        }

        [Authorize]
        public async Task<IActionResult> PackedManagement()
        {
            ViewBag.BreakDescriptions = await _breakService.GetBreakDescriptionsAsync();
            return PartialView("~/Views/Partial/_PackedManagement.cshtml");
        }

        [Authorize]
        public IActionResult CheckQtyToPack()
        {
            var apiSettings = _configuration.GetSection("SendQtySettings");
            var Persentage = apiSettings["Percentage"];
            ViewBag.Persentage = Persentage;
            return PartialView("~/Views/Partial/_CheckQtyToPack.cshtml");
        }

        [Authorize]
        public IActionResult Validation()
        {
            return PartialView("~/Views/Partial/_Validation.cshtml");
        }

        [Authorize]
        public async Task<IActionResult> UserManagement()
        {
            ViewBag.Employees = await _pISService.GetAvailableEmployeeAsync();
            List<UserModel> res = await _pISService.GetUser(new ReqUserModel());
            return PartialView("~/Views/Partial/_UserManagement.cshtml", res);
        }

        [Authorize]
        public async Task<IActionResult> PermissionManagement()
        {
            var res = await _permissionManagement.GetUserAsync();
            return PartialView("~/Views/Partial/_PermissionManagement.cshtml", res);
        }

        [Authorize]
        public async Task<IActionResult> EmployeeManagement()
        {
            ViewBag.Departments = await _pISService.GetDepartmentAsync();
            var res = await _pISService.GetEmployeeAsync();
            return PartialView("~/Views/Partial/_EmployeeManagement.cshtml", res);
        }

        [Authorize]
        public async Task<IActionResult> FormulaManagement()
        {
            //await _productionPlanningService.RegroupCustomer();
            ViewBag.CustomerGroups = await _productionPlanningService.GetCustomerGroupsAsync();
            ViewBag.ProductionTypes = await _productionPlanningService.GetProductionTypeAsync();
            ViewBag.PackMethods = await _productionPlanningService.GetPackMethodsAsync();
            var result = await _formulaManagementService.GetFormula(new Formula());
            return PartialView("~/Views/Partial/_FormulaManagement.cshtml", result);
        }

        [Authorize]
        public IActionResult AppSettings()
        {
            var SendQtySettings = _configuration.GetSection("SendQtySettings");
            var SendQtyPercentage = SendQtySettings["Percentage"];

            var SendToStoreSettings = _configuration.GetSection("SendToStoreSettings");
            var SendToStorePercentage = SendToStoreSettings["Percentage"];

            ViewBag.SendQtyPercentage = SendQtyPercentage;
            ViewBag.SendToStorePercentage = SendToStorePercentage;

            return PartialView("~/Views/Partial/_AppSetting.cshtml");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ImportReceiveNo(string receiveNo, string orderNo, string lotNo)
        {
            if (receiveNo == string.Empty && receiveNo == null) return BadRequest();
            var res = await _receiveManagementService.GetJPReceivedByReceiveNoAsync(receiveNo, orderNo, lotNo);
            return Ok(res);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> CancelImportReceiveNo(string receiveNo, string orderNo, string lotNo)
        {
            if (receiveNo == string.Empty && receiveNo == null) return BadRequest();
            var res = await _receiveManagementService.GetJPReceivedByReceiveNoAsync(receiveNo, orderNo, lotNo);
            return Ok(res);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetOrder(string orderNo, string lotNo, string custCode, DateTime fdate, DateTime edate, int page, int pageSize)
        {
            var result = await _orderManagementService.GetOrderAndLotByRangeAsync(orderNo, lotNo, custCode, fdate, edate, page, pageSize);
            return Ok(result);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetCustomLot(string lotNo)
        {
            var result = await _orderManagementService.GetCustomLotAsync(lotNo);
            if (result == null)
                return NotFound();

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
            return Ok(await _assignmentService.GetReceivedToAssignAsync(lotNo));
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
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPatch]
        [Authorize]
        public async Task<IActionResult> CancelUpdateLotItems([FromForm] string receiveNo, [FromForm] string[] orderNos, [FromForm] int[] receiveIds)
        {
            if (string.IsNullOrWhiteSpace(receiveNo) || receiveIds == null || receiveIds.Length == 0 || orderNos == null || orderNos.Length == 0)
                return BadRequest("ข้อมูลไม่ครบถ้วน");

            try
            {
                await _receiveManagementService.CancelUpdateLotItemsAsync(receiveNo, orderNos, receiveIds);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
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
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AssignToTable([FromForm] string lotNo, [FromForm] int[] receivedIDs, [FromForm] string tableId, [FromForm] string[] memberIds, [FromForm] bool hasPartTime, [FromForm] int workerNumber)
        {
            if (string.IsNullOrWhiteSpace(lotNo)) return BadRequest("ข้อมูลไม่ครบถ้วน");
            await _assignmentService.SyncAssignmentsForTableAsync(lotNo, int.Parse(tableId), receivedIDs, memberIds, hasPartTime, workerNumber);
            return Ok();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetTableMember(int tableID)
        {
            var result = await _assignmentService.GetTableMemberAsync(tableID);
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
            var result = await _returnService.GetTableToReturnAsync(LotNo);
            return Ok(result);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetRecievedToReturn(string LotNo, int TableID)
        {
            var result = await _returnService.GetRecievedToReturnAsync(LotNo, TableID);
            return Ok(result);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ReturnAssignment([FromForm] string lotNo, [FromForm] int[] assignmentIDs, [FromForm] decimal returnQty)
        {
            var result = await _returnService.ReturnReceivedAsync(lotNo, assignmentIDs, returnQty);
            return Ok(result);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetOrderToSendQty(string orderNo)
        {
            var result = await _checkQtyToSendService.GetOrderToSendQtyAsync(orderNo, null);
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
                await _checkQtyToSendService.DefineToPackAsync(request.OrderNo, request.Lots, User.GetUserId());
                return Ok("บันทึกข้อมูลเรียบร้อย");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> SendQtyToPackReport(string orderNo, PrintTo printTo)
        {
            _ = new SendToPackModel();
            SendToPackModel result;
            if (printTo == PrintTo.Export)
            {
                result = await _checkQtyToSendService.GetOrderToSendQtyWithPriceAsync(orderNo, User.GetUserId());
            }
            else
            {
                result = await _checkQtyToSendService.GetOrderToSendQtyAsync(orderNo, User.GetUserId());
            }

            var pdfBytes = _reportService.GenerateSendQtyToPackReport(result, printTo);

            var contentDisposition = $"inline; filename=DefineToPackReport_{DateTime.Now:yyyyMMdd}.pdf";
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

            var contentType = fullPath.GetContentType();
            if (string.IsNullOrEmpty(contentType))
                contentType = "application/octet-stream";

            var imageBytes = System.IO.File.ReadAllBytes(fullPath);
            return File(imageBytes, contentType);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetReceiveRow(string receiveNo)
        {
            var result = await _receiveManagementService.GetTopJPReceivedAsync(receiveNo, null, null);
            return Ok(result.FirstOrDefault());
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetReceiveList(string receiveNo, string orderNo, string lotNo)
        {
            var result = await _receiveManagementService.GetTopJPReceivedAsync(receiveNo, orderNo, lotNo);
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
                UserModel res = await _pISService.ValidateApproverAsync(username, password);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddNewBreakDescription([FromForm] string breakDescription)
        {
            if (string.IsNullOrWhiteSpace(breakDescription)) return BadRequest();

            try
            {
                var res = await _breakService.AddNewBreakDescription(breakDescription);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> GetBreak([FromBody] BreakAndLostFilterModel breakAndLostFilterModel)
        {
            List<LostAndRepairModel> result = await _breakService.GetBreakAsync(breakAndLostFilterModel);
            return Ok(result);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> BreakReport([FromBody] BreakAndLostFilterModel breakAndLostFilterModel)
        {

            List<LostAndRepairModel> result = await _breakService.GetBreakAsync(breakAndLostFilterModel);
            byte[] pdfBytes = _reportService.GenerateBreakReport(result);
            await _breakService.PintedBreakReport(breakAndLostFilterModel.BreakIDs);

            string contentDisposition = $"inline; filename=BreakReport_{DateTime.Now:yyyyMMdd}.pdf";
            Response.Headers.Append("Content-Disposition", contentDisposition);

            return File(pdfBytes, "application/pdf");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> GetLost([FromBody] BreakAndLostFilterModel breakAndLostFilterModel)
        {
            List<LostAndRepairModel> result = await _lostService.GetLostAsync(breakAndLostFilterModel);
            return Ok(result);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> GetLeaderTabel([FromBody] BreakAndLostFilterModel breakAndLostFilterModel)
        {
            List<AssignedWorkTableModel> result = await _lostService.GetTableLeaderAsync(breakAndLostFilterModel.LotNo);
            return Ok(result);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> LostReport([FromBody] BreakAndLostFilterModel breakAndLostFilterModel)
        {
            UserModel user = await _orderManagementService.ValidateApporverAsync(breakAndLostFilterModel.Username, breakAndLostFilterModel.Password);
            List<LostAndRepairModel> result = await _lostService.GetLostAsync(breakAndLostFilterModel);
            byte[] pdfBytes = _reportService.GenerateLostReport(result, user);
            await _lostService.PintedLostReport(breakAndLostFilterModel.LostIDs);

            string contentDisposition = $"inline; filename=LostReport_{DateTime.Now:yyyyMMdd}.pdf";
            Response.Headers.Append("Content-Disposition", contentDisposition);

            return File(pdfBytes, "application/pdf");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> GetUser([FromBody] ReqUserModel? reqUserModel = null)
        {
            try
            {
                List<UserModel> res = await _pISService.GetUser(reqUserModel);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> AvailableEmployee(string receiveNo)
        {
            var result = await _pISService.GetAvailableEmployeeAsync();
            return Ok(result);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddNewUser([FromBody] UserModel userModel)
        {
            try
            {
                var res = await _pISService.AddNewUser(userModel);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

        }

        [HttpPatch]
        [Authorize]
        public async Task<IActionResult> EditUser([FromBody] UserModel userModel)
        {
            try
            {
                var res = await _pISService.EditUser(userModel);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

        }

        [HttpPatch]
        [Authorize]
        public async Task<IActionResult> ToggleUserStatus([FromBody] UserModel userModel)
        {
            try
            {
                var res = await _pISService.ToggleUserStatus(userModel);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddLost([FromForm] string lotNo, [FromForm] double lostQty, [FromForm] int leaderID)
        {
            try
            {
                await _lostService.AddLostAsync(lotNo, lostQty, leaderID);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddBreak([FromForm] string lotNo, [FromForm] double breakQty, [FromForm] int breakDes)
        {
            try
            {
                await _breakService.AddBreakAsync(lotNo, breakQty, breakDes);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetOrderToStore(string orderNo)
        {
            var result = await _packedMangementService.GetOrderToStoreAsync(orderNo);
            return Ok(result);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SendToStore([FromBody] SendStockInput sendStockInput)
        {
            try
            {
                var res = await _packedMangementService.SendStockAsync(sendStockInput);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ConfirmToSendStore([FromForm] string[] lotNos, [FromForm] string userId)
        {
            try
            {
                var a = await _packedMangementService.ConfirmToSendStoreAsync(lotNos, userId);
                var b = await _packedMangementService.ConfirmToSendMeltAsync(lotNos, userId);
                var c = await _packedMangementService.ConfirmToSendExportAsync(lotNos, userId);
                var d = await _packedMangementService.ConfirmToSendLostAsync(lotNos, userId);

                string message = $"Store: {(a.IsSuccess ? "O" : "X")}\n" +
                                 $"Melt: {(b.IsSuccess ? "O" : "X")}\n" +
                                 $"Export: {(c.IsSuccess ? "O" : "X")}\n" +
                                 $"Lost: {(d.IsSuccess ? "O" : "X")}";

                BaseResponseModel responseModel = new()
                {
                    IsSuccess = a.IsSuccess || b.IsSuccess || c.IsSuccess || d.IsSuccess,
                    Message = message
                };

                return Ok(responseModel);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponseModel
                {
                    IsSuccess = false,
                    Message = $"เกิดข้อผิดพลาด: {ex.Message}"
                });
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> PrintSendToAllReport([FromForm] string[] lotNos, [FromForm] string userId)
        {
            try
            {
                List<TempPack> result = await _packedMangementService.GetAllDocToPrint(lotNos, userId);
                byte[] pdfBytes = _reportService.GenerateSenToReport(result);

                string contentDisposition = $"inline; filename=AllSendToReport_{DateTime.Now:yyyyMMdd}.pdf";
                Response.Headers.Append("Content-Disposition", contentDisposition);

                return File(pdfBytes, "application/pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> GetComparedInvoice([FromBody] ComparedInvoiceFilterModel comparedInvoiceFilterModel)
        {
            try
            {
                List<ComparedInvoiceModel> result = await _auditService.GetFilteredInvoice(comparedInvoiceFilterModel);
                byte[] pdfBytes = _reportService.GenerateComparedInvoiceReport(comparedInvoiceFilterModel, result, comparedInvoiceFilterModel.InvoiceType);

                string contentDisposition = $"inline; filename=ComINV{DateTime.Now:yyyyMMdd}.pdf";
                Response.Headers.Append("Content-Disposition", contentDisposition);

                return File(pdfBytes, "application/pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> GetUnallocatedQuentityToStore([FromBody] ComparedInvoiceFilterModel comparedInvoiceFilterModel)
        {
            try
            {
                var a = await _auditService.GetUnallocatedQuentityToStore(comparedInvoiceFilterModel);
                //byte[] pdfBytes = _reportService.GenerateComparedInvoiceReport(comparedInvoiceFilterModel, result, comparedInvoiceFilterModel.InvoiceType);

                //string contentDisposition = $"inline; filename=ComINV{DateTime.Now:yyyyMMdd}.pdf";
                //Response.Headers.Append("Content-Disposition", contentDisposition);

                //return File(pdfBytes, "application/pdf");
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> GetFormula([FromBody] Formula formula)
        {
            var result = await _formulaManagementService.GetFormula(formula);
            return Ok(result);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddNewFormula([FromBody] Formula formula)
        {
            try
            {
                var res = await _formulaManagementService.AddNewFormula(formula);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPatch]
        [Authorize]
        public async Task<IActionResult> EditFormula([FromBody] Formula formula)
        {
            try
            {
                var res = await _formulaManagementService.UpdateFormula(formula);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPatch]
        [Authorize]
        public async Task<IActionResult> ToggleFormulaStatus([FromBody] Formula formula)
        {
            try
            {
                var res = await _formulaManagementService.ToggleFormulaStatus(formula.FormulaId);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUserPermission(int userId)
        {
            var permissions = await _permissionManagement.GetPermissionAsync();
            var mapping = await _permissionManagement.GetMappingPermissionAsync(userId);

            var result = permissions.Select(p => new
            {
                p.PermissionId,
                p.Name,
                Enabled = mapping.Any(m => m.PermissionId == p.PermissionId)
            }).ToList();

            return Ok(result);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateUserPermission([FromBody] UpdatePermissionModel model)
        {
            var result = await _permissionManagement.UpdatePermissionAsync(model);
            return Ok(result);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetEmployee()
        {
            try
            {
                var res = await _pISService.GetEmployeeAsync();
                return Ok(res);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> GetEmployeeByID([FromBody] ResEmployeeModel resEmployeeModel)
        {
            try
            {
                var emplist = await _pISService.GetEmployeeAsync();
                if (emplist == null)
                {
                    return Ok(new BaseResponseModel
                    {
                        IsSuccess = false,
                        Message = "ไม่พบข้อมูลพนักงาน"
                    });
                }

                var emp = emplist.FirstOrDefault(e => e.EmployeeID == resEmployeeModel.EmployeeID);
                return Ok(emp);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddNewEmployee([FromBody] ResEmployeeModel resEmployeeModel)
        {
            try
            {
                var res = await _pISService.AddNewEmployee(resEmployeeModel);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPatch]
        [Authorize]
        public async Task<IActionResult> EditEmployee([FromBody] ResEmployeeModel resEmployeeModel)
        {
            try
            {
                var res = await _pISService.EditEmployee(resEmployeeModel);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPatch]
        [Authorize]
        public async Task<IActionResult> ToggleEmployeelaStatus([FromBody] ResEmployeeModel resEmployeeModel)
        {
            try
            {
                var res = await _pISService.ToggleEmployeeStatus(resEmployeeModel);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateAppSettings([FromBody] UpdateAppSettingsModel updateAppSettingsModel, [FromServices] IWebHostEnvironment env)
        {
            var environmentName = env.EnvironmentName;

            var fileName = environmentName switch
            {
                "Production" => "appsettings.Production.json",
                "Staging" => "appsettings.Staging.json",
                _ => "appsettings.Development.json"
            };

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);

            if (!System.IO.File.Exists(filePath))
                return NotFound($"ไม่พบไฟล์ {fileName}");

            try
            {
                string json = await System.IO.File.ReadAllTextAsync(filePath);
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement.Clone();

                var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(json)!;

                var sendQtySettings = JsonSerializer.Deserialize<Dictionary<string, object>>(
                    root.GetProperty("SendQtySettings").GetRawText()
                )!;
                sendQtySettings["Persentage"] = updateAppSettingsModel.ChxQtyPersentage;

                var sendToStoreSettings = JsonSerializer.Deserialize<Dictionary<string, object>>(
                    root.GetProperty("SendToStoreSettings").GetRawText()
                )!;
                sendToStoreSettings["Persentage"] = updateAppSettingsModel.MinWgPersentage;

                dict["SendQtySettings"] = sendQtySettings;
                dict["SendToStoreSettings"] = sendToStoreSettings;

                string updatedJson = JsonSerializer.Serialize(dict, new JsonSerializerOptions { WriteIndented = true });
                await System.IO.File.WriteAllTextAsync(filePath, updatedJson);

                (_configuration as IConfigurationRoot)?.Reload();

                return Ok(new BaseResponseModel
                {
                    IsSuccess = true,
                    Message = $"อัปเดต {fileName} สำเร็จ (Environment: {environmentName})"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponseModel
                {
                    IsSuccess = false,
                    Message = $"เกิดข้อผิดพลาด: {ex.Message}"
                });
            }
        }


    }
}
