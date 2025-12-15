using JPStockPacking.Data.JPDbContext;
using JPStockPacking.Data.SPDbContext;
using JPStockPacking.Models;
using JPStockPacking.Services.Helper;
using JPStockPacking.Services.Implement;
using JPStockPacking.Services.Interface;
using JPStockPacking.Services.Middleware;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

builder.Services.AddDbContext<JPDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("JPDBEntries")));
builder.Services.AddDbContext<SPDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("SPDBEntries")));

builder.Services.Configure<AppSettingModel>(builder.Configuration.GetSection("AppSettings"));
builder.Services.Configure<SendQtyModel>(builder.Configuration.GetSection("SendQtySettings"));

builder.Services.AddTransient<TokenHandler>();
builder.Services.AddHttpClient("ApiClient").AddHttpMessageHandler<TokenHandler>();

builder.Host.UseSerilog((context, configuration) => configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.AddMemoryCache();
builder.Services.AddScoped<ICacheService, CacheService>();
builder.Services.AddScoped<ICookieAuthService, CookieAuthService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IApiClientService, ApiClientService>();
builder.Services.AddScoped<IPISService, PISService>();
builder.Services.AddScoped<IOrderManagementService, OrderManagementService>();
builder.Services.AddScoped<IProductionPlanningService, ProductionPlanningService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IReceiveManagementService, ReceiveManagementService>();
builder.Services.AddScoped<IAssignmentService, AssignmentService>();
builder.Services.AddScoped<ICheckQtyToSendService, CheckQtyToSendService>();
builder.Services.AddScoped<IBreakService, BreakService>();
builder.Services.AddScoped<ILostService, LostService>();
builder.Services.AddScoped<IPackedMangementService, PackedMangementService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IFormulaManagementService, FormulaManagementService>();
builder.Services.AddScoped<IPermissionManagement, PermissionManagement>();
builder.Services.AddScoped<IReturnService, ReturnService>();

builder.Services.AddAuthentication("AppCookieAuth")
    .AddCookie("AppCookieAuth", options =>
    {
        options.Cookie.Name = "JPApp.Auth";
        options.LoginPath = "/Auth/Login";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<AutoRefreshTokenMiddleware>();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
