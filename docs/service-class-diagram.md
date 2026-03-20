# Class Diagram - Service Layer

## Overview (Architecture)

```mermaid
classDiagram
    direction TB

    %% ===== DbContexts =====
    class SPDbContext { <<DbContext>> }
    class JPDbContext { <<DbContext>> }
    class SWDbContext { <<DbContext>> }
    class BMDbContext { <<DbContext>> }

    %% ===== Auth Group =====
    class IAuthService { <<interface>> }
    class ICookieAuthService { <<interface>> }
    class AuthService
    class CookieAuthService

    AuthService ..|> IAuthService
    CookieAuthService ..|> ICookieAuthService
    AuthService ..> ICookieAuthService : uses

    %% ===== Infrastructure =====
    class IApiClientService { <<interface>> }
    class ICacheService { <<interface>> }
    class ApiClientService
    class CacheService

    ApiClientService ..|> IApiClientService
    CacheService ..|> ICacheService

    %% ===== PIS / User =====
    class IPISService { <<interface>> }
    class IPermissionManagement { <<interface>> }
    class PISService
    class PermissionManagement

    PISService ..|> IPISService
    PermissionManagement ..|> IPermissionManagement
    PISService ..> IApiClientService : uses
    PISService ..> ICacheService : uses
    PISService ..> SPDbContext : uses
    PermissionManagement ..> IPISService : uses
    PermissionManagement ..> SPDbContext : uses

    %% ===== Order & Lot =====
    class IOrderManagementService { <<interface>> }
    class IProductionPlanningService { <<interface>> }
    class ICheckQtyToSendService { <<interface>> }
    class OrderManagementService
    class ProductionPlanningService
    class CheckQtyToSendService

    OrderManagementService ..|> IOrderManagementService
    ProductionPlanningService ..|> IProductionPlanningService
    CheckQtyToSendService ..|> ICheckQtyToSendService

    OrderManagementService ..> JPDbContext : uses
    OrderManagementService ..> SPDbContext : uses
    OrderManagementService ..> IPISService : uses
    OrderManagementService ..> IReceiveManagementService : uses

    ProductionPlanningService ..> JPDbContext : uses
    ProductionPlanningService ..> SPDbContext : uses
    ProductionPlanningService ..> BMDbContext : uses

    CheckQtyToSendService ..> JPDbContext : uses
    CheckQtyToSendService ..> SPDbContext : uses
    CheckQtyToSendService ..> IPISService : uses

    %% ===== Receive =====
    class IReceiveManagementService { <<interface>> }
    class ISampleReceiveManagementService { <<interface>> }
    class ReceiveManagementService
    class SampleReceiveManagementService

    ReceiveManagementService ..|> IReceiveManagementService
    SampleReceiveManagementService ..|> ISampleReceiveManagementService

    ReceiveManagementService ..> JPDbContext : uses
    ReceiveManagementService ..> SPDbContext : uses
    ReceiveManagementService ..> SWDbContext : uses
    ReceiveManagementService ..> IProductionPlanningService : uses
    ReceiveManagementService ..> IPackedMangementService : uses

    SampleReceiveManagementService ..> JPDbContext : uses
    SampleReceiveManagementService ..> SPDbContext : uses
    SampleReceiveManagementService ..> SWDbContext : uses

    %% ===== Packing =====
    class IPackedMangementService { <<interface>> }
    class PackedMangementService

    PackedMangementService ..|> IPackedMangementService
    PackedMangementService ..> JPDbContext : uses
    PackedMangementService ..> SPDbContext : uses
    PackedMangementService ..> IPISService : uses

    %% ===== Assignment =====
    class IAssignmentService { <<interface>> }
    class IReturnService { <<interface>> }
    class AssignmentService
    class ReturnService

    AssignmentService ..|> IAssignmentService
    ReturnService ..|> IReturnService

    AssignmentService ..> SPDbContext : uses
    AssignmentService ..> IPISService : uses
    ReturnService ..> SPDbContext : uses

    %% ===== Quality Control =====
    class IBreakService { <<interface>> }
    class ILostService { <<interface>> }
    class IAuditService { <<interface>> }
    class ICancelReceiveService { <<interface>> }
    class BreakService
    class LostService
    class AuditService
    class CancelReceiveService

    BreakService ..|> IBreakService
    LostService ..|> ILostService
    AuditService ..|> IAuditService
    CancelReceiveService ..|> ICancelReceiveService

    BreakService ..> JPDbContext : uses
    BreakService ..> SPDbContext : uses
    LostService ..> SPDbContext : uses
    LostService ..> IPISService : uses
    AuditService ..> JPDbContext : uses
    AuditService ..> SPDbContext : uses
    CancelReceiveService ..> JPDbContext : uses
    CancelReceiveService ..> SPDbContext : uses

    %% ===== Report =====
    class IReportService { <<interface>> }
    class IProductTypeService { <<interface>> }
    class ReportService
    class ProductTypeService

    ReportService ..|> IReportService
    ProductTypeService ..|> IProductTypeService
    ProductTypeService ..> SPDbContext : uses
```

---

## Group 1: Authentication

```mermaid
classDiagram
    class IAuthService {
        <<interface>>
        +LoginUserAsync(username, password, rememberMe) Task~LoginResult~
        +RefreshTokenAsync() Task~RefreshTokenResult~
        +LogoutAsync() Task~bool~
    }

    class ICookieAuthService {
        <<interface>>
        +SignInAsync(context, id, username, rememberMe) Task
        +SignOutAsync(context) Task
    }

    class AuthService {
        -IHttpContextAccessor contextAccessor
        -ICookieAuthService cookieAuthService
        -IConfiguration configuration
        -ILogger logger
        +LoginUserAsync(username, password, rememberMe) Task~LoginResult~
        +RefreshTokenAsync() Task~RefreshTokenResult~
        +LogoutAsync() Task~bool~
    }

    class CookieAuthService {
        -SPDbContext sPDbContext
        +SignInAsync(context, id, username, rememberMe) Task
        +SignOutAsync(context) Task
    }

    AuthService ..|> IAuthService
    CookieAuthService ..|> ICookieAuthService
    AuthService ..> ICookieAuthService : uses
    CookieAuthService ..> SPDbContext : uses

    class SPDbContext { <<DbContext>> }
```

---

## Group 2: Infrastructure / Utility

```mermaid
classDiagram
    class IApiClientService {
        <<interface>>
        +GetAsync~T~(url, token) Task~BaseResponseModel~T~~
        +PostAsync~T~(url, payload, token) Task~BaseResponseModel~T~~
        +PostAsync(url, payload, token) Task~BaseResponseModel~
        +PatchAsync(url, payload, token) Task~BaseResponseModel~
    }

    class ICacheService {
        <<interface>>
        +GetOrCreateAsync~T~(cacheKey, factory, absoluteExpiration) Task~T~
        +Remove(cacheKey) void
        +Clear() void
    }

    class ApiClientService {
        -IConfiguration configuration
        -IHttpContextAccessor contextAccessor
        -IHttpClientFactory httpClientFactory
        +GetAsync~T~(url, token) Task~BaseResponseModel~T~~
        +PostAsync~T~(url, payload, token) Task~BaseResponseModel~T~~
        +PostAsync(url, payload, token) Task~BaseResponseModel~
        +PatchAsync(url, payload, token) Task~BaseResponseModel~
    }

    class CacheService {
        -IMemoryCache cache
        -ILogger logger
        +GetOrCreateAsync~T~(cacheKey, factory, absoluteExpiration) Task~T~
        +Remove(cacheKey) void
        +Clear() void
    }

    ApiClientService ..|> IApiClientService
    CacheService ..|> ICacheService
```

---

## Group 3: PIS / User / Permission

```mermaid
classDiagram
    class IPISService {
        <<interface>>
        +GetEmployeeAsync() Task~List~ResEmployeeModel~~
        +GetAvailableEmployeeAsync() Task~List~ResEmployeeModel~~
        +GetDepartmentAsync() Task~List~DepartmentModel~~
        +ValidateApproverAsync(username, password) Task~UserModel~
        +GetAllUser() Task~List~UserModel~~
        +GetUser(payload) Task~List~UserModel~~
        +AddNewUser(payload) Task~BaseResponseModel~
        +EditUser(payload) Task~BaseResponseModel~
        +ToggleUserStatus(payload) Task~BaseResponseModel~
        +AddNewEmployee(payload) Task~BaseResponseModel~
        +EditEmployee(payload) Task~BaseResponseModel~
        +ToggleEmployeeStatus(payload) Task~BaseResponseModel~
    }

    class IPermissionManagement {
        <<interface>>
        +GetPermissionAsync() Task~List~Permission~~
        +GetUserAsync() Task~List~UserModel~~
        +GetMappingPermissionAsync(UserID) Task~List~MappingPermission~~
        +UpdatePermissionAsync(model) Task~BaseResponseModel~
    }

    class PISService {
        -IConfiguration configuration
        -IApiClientService apiClientService
        -ICacheService cacheService
        -ILogger logger
        -SPDbContext sPDbContext
    }

    class PermissionManagement {
        -SPDbContext sPDbContext
        -IPISService pISService
    }

    PISService ..|> IPISService
    PermissionManagement ..|> IPermissionManagement
    PISService ..> IApiClientService : uses
    PISService ..> ICacheService : uses
    PISService ..> SPDbContext : uses
    PermissionManagement ..> IPISService : uses
    PermissionManagement ..> SPDbContext : uses

    class IApiClientService { <<interface>> }
    class ICacheService { <<interface>> }
    class SPDbContext { <<DbContext>> }
```

---

## Group 4: Order & Production Planning

```mermaid
classDiagram
    class IOrderManagementService {
        <<interface>>
        +GetOrderAndLotByRangeAsync(...) Task~PagedScheduleListModel~
        +GetReceivedAsync(lotNo) Task~List~ReceivedListModel~~
        +GetCustomLotAsync(lotNo) Task~CustomLot~
        +GetTableAsync() Task~List~WorkTable~~
        +UpdateAllReceivedItemsAsync(receiveNo) Task
        +ValidateApporverAsync(username, password) Task~UserModel~
        +ImportOrderAsync() Task
    }

    class IProductionPlanningService {
        <<interface>>
        +GetOrderToPlan(FromDate, ToDate) Task~List~OrderPlanModel~~
        +RegroupCustomer() Task
        +CalLotOperateDay(TtQty, ProdType, Article, OrderNo) double
    }

    class ICheckQtyToSendService {
        <<interface>>
        +GetOrderToSendQtyAsync(orderNo, userid) Task~SendToPackModel~
        +GetOrderToSendQtyWithPriceAsync(orderNo, userid) Task~SendToPackModel~
        +DefineToPackAsync(orderNo, lots, userid) Task
    }

    class OrderManagementService {
        -JPDbContext jPDbContext
        -SPDbContext sPDbContext
        -IPISService pISService
        -IReceiveManagementService receiveManagementService
        -IWebHostEnvironment webHostEnvironment
    }

    class ProductionPlanningService {
        -JPDbContext jPDbContext
        -SPDbContext sPDbContext
        -BMDbContext bMDbContext
    }

    class CheckQtyToSendService {
        -JPDbContext jPDbContext
        -SPDbContext sPDbContext
        -IPISService pISService
        -IConfiguration configuration
    }

    OrderManagementService ..|> IOrderManagementService
    ProductionPlanningService ..|> IProductionPlanningService
    CheckQtyToSendService ..|> ICheckQtyToSendService

    OrderManagementService ..> IReceiveManagementService : uses
    OrderManagementService ..> IPISService : uses

    class IReceiveManagementService { <<interface>> }
    class IPISService { <<interface>> }
    class JPDbContext { <<DbContext>> }
    class SPDbContext { <<DbContext>> }
    class BMDbContext { <<DbContext>> }
```

---

## Group 5: Receive Management

```mermaid
classDiagram
    class IReceiveManagementService {
        <<interface>>
        +GetTopJPReceivedAsync(receiveNo, orderNo, lotNo) Task~List~ReceivedListModel~~
        +GetJPReceivedByReceiveNoAsync(receiveNo, orderNo, lotNo) Task~List~ReceivedListModel~~
        +UpdateLotItemsAsync(receiveNo, orderNos, receiveIds) Task
        +CancelUpdateLotItemsAsync(receiveNo, orderNos, receiveIds) Task
        +GetJPLotAsync(newOrders) Task~List~Lot~~
        +UpdateReceiveHeaderStatusAsync(receiveNo) Task
    }

    class ISampleReceiveManagementService {
        <<interface>>
        +GetTopJPReceivedAsync(receiveNo, orderNo, lotNo) Task~List~ReceivedListModel~~
        +GetJPReceivedByReceiveNoAsync(receiveNo, orderNo, lotNo) Task~List~ReceivedListModel~~
        +UpdateLotItemsAsync(receiveNo, orderNos, receiveIds) Task
        +CancelUpdateLotItemsAsync(receiveNo, orderNos, receiveIds) Task
    }

    class ReceiveManagementService {
        -JPDbContext jPDbContext
        -SPDbContext sPDbContext
        -SWDbContext sWDbContext
        -IProductionPlanningService productionPlanningService
        -IPackedMangementService packedMangementService
    }

    class SampleReceiveManagementService {
        -JPDbContext jPDbContext
        -SPDbContext sPDbContext
        -SWDbContext sWDbContext
    }

    ReceiveManagementService ..|> IReceiveManagementService
    SampleReceiveManagementService ..|> ISampleReceiveManagementService
    ReceiveManagementService ..> IProductionPlanningService : uses
    ReceiveManagementService ..> IPackedMangementService : uses

    class IProductionPlanningService { <<interface>> }
    class IPackedMangementService { <<interface>> }
    class JPDbContext { <<DbContext>> }
    class SPDbContext { <<DbContext>> }
    class SWDbContext { <<DbContext>> }
```

---

## Group 6: Packing & Assignment

```mermaid
classDiagram
    class IPackedMangementService {
        <<interface>>
        +GetOrderToStoreAsync(orderNo) Task~List~OrderToStoreModel~~
        +GetOrderToStoreByLotAsync(lotNo) Task~OrderToStoreModel~
        +SendStockAsync(input) Task~BaseResponseModel~
        +ConfirmToSendStoreAsync(lotNos, userId) Task~BaseResponseModel~
        +ConfirmToSendMeltAsync(lotNos, userId) Task~BaseResponseModel~
        +ConfirmToSendLostAsync(lotNos, userId) Task~BaseResponseModel~
        +ConfirmToSendExportAsync(lotNos, userId) Task~BaseResponseModel~
        +ConfirmToSendShowroomAsync(lotNos, userId) Task~BaseResponseModel~
        +GetAllDocToPrint(lotNos, userid) Task~List~TempPack~~
        +GetDocToPrintByType(lotNos, userid, sendType) Task~List~TempPack~~
        +UpdateArticleAsync(orderNo) Task
        +UpdateOrderSuccessAsync(orderNo) Task
    }

    class IAssignmentService {
        <<interface>>
        +GetTableMemberAsync(tableID) Task~List~TableMemberModel~~
        +GetReceivedToAssignAsync(lotNo) Task~List~ReceivedListModel~~
        +SyncAssignmentsForTableAsync(lotNo, tableId, receivedIds, memberIds, hasPartTime, workerNumber) Task
    }

    class IReturnService {
        <<interface>>
        +ReturnReceivedAsync(lotNo, assignmentIDs, returnQty) Task~BaseResponseModel~
        +GetTableToReturnAsync(LotNo) Task~List~TableModel~~
        +GetRecievedToReturnAsync(LotNo, TableID) Task~List~ReceivedListModel~~
    }

    class PackedMangementService {
        -JPDbContext jPDbContext
        -SPDbContext sPDbContext
        -IPISService pISService
        -IConfiguration configuration
    }

    class AssignmentService {
        -SPDbContext sPDbContext
        -IPISService pISService
    }

    class ReturnService {
        -SPDbContext sPDbContext
    }

    PackedMangementService ..|> IPackedMangementService
    AssignmentService ..|> IAssignmentService
    ReturnService ..|> IReturnService
    PackedMangementService ..> IPISService : uses
    AssignmentService ..> IPISService : uses

    class IPISService { <<interface>> }
    class JPDbContext { <<DbContext>> }
    class SPDbContext { <<DbContext>> }
```

---

## Group 7: Quality Control & Audit

```mermaid
classDiagram
    class IAuditService {
        <<interface>>
        +GetFilteredInvoice(filter) Task~List~ComparedInvoiceModel~~
        +GetUnallocatedQuentityToStore(filter) Task~List~UnallocatedQuantityModel~~
        +MarkInvoiceAsRead(InvoiceNo, userId) Task~BaseResponseModel~
        +GetConfirmedInvoice(InvoiceNo) Task~List~ComparedInvoiceModel~~
        +GetIsMarked(InvoiceNo) Task~BaseResponseModel~
        +GetSendLostCheckList(filter) Task~List~SendLostCheckModel~~
    }

    class IBreakService {
        <<interface>>
        +GetBreakAsync(filter) Task~List~LostAndRepairModel~~
        +AddBreakAsync(lotNo, breakQty, breakDes) Task
        +GetBreakDescriptionsAsync() Task~List~BreakDescription~~
        +AddNewBreakDescription(breakDescription) Task~List~BreakDescription~~
        +PintedBreakReport(BreakIDs) Task
    }

    class ILostService {
        <<interface>>
        +GetLostAsync(filter) Task~List~LostAndRepairModel~~
        +AddLostAsync(lotNo, lostQty, leaderID) Task
        +PintedLostReport(LostIDs) Task
        +GetTableLeaderAsync(LotNo) Task~List~AssignedWorkTableModel~~
    }

    class ICancelReceiveService {
        <<interface>>
        +GetTopSJ1JPReceivedAsync(receiveNo, orderNo, lotNo) Task~List~ReceivedListModel~~
        +GetSJ1JPReceivedByReceiveNoAsync(receiveNo, orderNo, lotNo) Task~List~ReceivedListModel~~
        +CancelSJ1ByReceiveNoAsync(receiveNo, userId) Task~BaseResponseModel~
        +CancelSJ1ByLotNoAsync(receiveNo, lotNos, userId) Task~BaseResponseModel~
        +GetTopSJ2JPReceivedAsync(receiveNo, orderNo, lotNo) Task~List~ReceivedListModel~~
        +CancelSJ2ByReceiveNoAsync(receiveNo, userId) Task~BaseResponseModel~
        +CancelSJ2ByLotNoAsync(receiveNo, lotNos, userId) Task~BaseResponseModel~
        +GetTopSendLostReceivedAsync(receiveNo, orderNo, lotNo) Task~List~ReceivedListModel~~
        +CancelSendLostByReceiveNoAsync(receiveNo, userId) Task~BaseResponseModel~
        +CancelSendLostByLotNoAsync(receiveNo, lotNos, userId) Task~BaseResponseModel~
        +GetTopExportReceivedAsync(receiveNo, orderNo, lotNo) Task~List~ReceivedListModel~~
        +CancelExportByReceiveNoAsync(receiveNo, userId) Task~BaseResponseModel~
        +CancelExportByLotNoAsync(receiveNo, lotNos, userId) Task~BaseResponseModel~
        +GetTopShowroomReceivedAsync(receiveNo, orderNo, lotNo) Task~List~ReceivedListModel~~
        +CancelShowroomByReceiveNoAsync(receiveNo, userId) Task~BaseResponseModel~
        +CancelShowroomByLotNoAsync(receiveNo, lotNos, userId) Task~BaseResponseModel~
    }

    class AuditService {
        -JPDbContext jPDbContext
        -SPDbContext sPDbContext
    }

    class BreakService {
        -JPDbContext jPDbContext
        -SPDbContext sPDbContext
    }

    class LostService {
        -SPDbContext sPDbContext
        -IPISService pISService
    }

    class CancelReceiveService {
        -SPDbContext sPDbContext
        -JPDbContext jPDbContext
    }

    AuditService ..|> IAuditService
    BreakService ..|> IBreakService
    LostService ..|> ILostService
    CancelReceiveService ..|> ICancelReceiveService
    LostService ..> IPISService : uses

    class IPISService { <<interface>> }
    class JPDbContext { <<DbContext>> }
    class SPDbContext { <<DbContext>> }
```

---

## Group 8: Report & Master Data

```mermaid
classDiagram
    class IReportService {
        <<interface>>
        +GenerateSendQtyToPackReport(model, printTo) byte[]
        +GenerateBreakReport(model) byte[]
        +GenerateLostReport(model, userModel) byte[]
        +GenerateSenToReport(model) byte[]
        +GenerateComparedInvoiceReport(filter, model, invoiceType) byte[]
        +GenerateUnallocatedInvoiceReport(filter, model) byte[]
    }

    class IProductTypeService {
        <<interface>>
        +GetAllAsync() Task~List~ProductType~~
        +GetByIdAsync(id) Task~ProductType~
        +AddAsync(model) Task~BaseResponseModel~
        +UpdateAsync(model) Task~BaseResponseModel~
        +ToggleStatusAsync(id) Task~BaseResponseModel~
    }

    class ReportService {
        -IWebHostEnvironment webHostEnvironment
        +GenerateSendQtyToPackReport(model, printTo) byte[]
        +GenerateBreakReport(model) byte[]
        +GenerateLostReport(model, userModel) byte[]
        +GenerateSenToReport(model) byte[]
        +GenerateComparedInvoiceReport(filter, model, invoiceType) byte[]
        +GenerateUnallocatedInvoiceReport(filter, model) byte[]
    }

    class ProductTypeService {
        -SPDbContext sPDbContext
        +GetAllAsync() Task~List~ProductType~~
        +GetByIdAsync(id) Task~ProductType~
        +AddAsync(model) Task~BaseResponseModel~
        +UpdateAsync(model) Task~BaseResponseModel~
        +ToggleStatusAsync(id) Task~BaseResponseModel~
    }

    class ReportExtension {
        <<static>>
        +CreateLotItemCard(container, item) void
        +CreateLotSizeItemCard(container, item) void
        +CreateLotItemWithPriceCard(container, item) void
        +BreakReportContent(container, model) void
        +LostReportContent(container, model, userModel) void
        +SendToReportContent(container, model) void
        +RenderHeaderImage(container, localPath, imgPath) void
        +RenderItemImage(container, localPath, imgPath) void
    }

    ReportService ..|> IReportService
    ProductTypeService ..|> IProductTypeService
    ReportService ..> ReportExtension : uses

    class SPDbContext { <<DbContext>> }
```

---

## สรุป Service Layer

| กลุ่ม | Interface | Implementation | DbContexts ที่ใช้ |
|---|---|---|---|
| Authentication | IAuthService, ICookieAuthService | AuthService, CookieAuthService | SP |
| Infrastructure | IApiClientService, ICacheService | ApiClientService, CacheService | - |
| PIS / User | IPISService, IPermissionManagement | PISService, PermissionManagement | SP |
| Order & Planning | IOrderManagementService, IProductionPlanningService, ICheckQtyToSendService | OrderManagementService, ProductionPlanningService, CheckQtyToSendService | JP, SP, BM |
| Receive | IReceiveManagementService, ISampleReceiveManagementService | ReceiveManagementService, SampleReceiveManagementService | JP, SP, SW |
| Packing & Assignment | IPackedMangementService, IAssignmentService, IReturnService | PackedMangementService, AssignmentService, ReturnService | JP, SP |
| Quality Control | IAuditService, IBreakService, ILostService, ICancelReceiveService | AuditService, BreakService, LostService, CancelReceiveService | JP, SP |
| Report & Master | IReportService, IProductTypeService | ReportService, ProductTypeService | SP |
