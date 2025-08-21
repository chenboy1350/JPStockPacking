using JPStockPacking.Data.JPDbContext;
using JPStockPacking.Data.SPDbContext;
using JPStockPacking.Services.Implement;
using JPStockPacking.Services.Interface;
using JPStockPacking.Services.Middleware;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

builder.Services.AddDbContext<JPDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("JPDBEntries")));
builder.Services.AddDbContext<SPDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("SPDBEntries")));

builder.Services.AddScoped<ICookieAuthService, CookieAuthService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IApiClientService, ApiClientService>();
builder.Services.AddScoped<IPISService, PISService>();
builder.Services.AddScoped<IOrderManagementService, OrderManagementService>();
builder.Services.AddScoped<IProductionPlanningService, ProductionPlanningService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

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
