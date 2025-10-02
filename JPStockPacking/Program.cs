using JPStockPacking.Data.JPDbContext;
using JPStockPacking.Data.SPDbContext;
using JPStockPacking.Models;
using JPStockPacking.Services.Helper;
using JPStockPacking.Services.Implement;
using JPStockPacking.Services.Interface;
using JPStockPacking.Services.Middleware;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

builder.Services.AddDbContext<JPDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("JPDBEntries")));
builder.Services.AddDbContext<SPDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("SPDBEntries")));

builder.Services.Configure<AppSettingModel>(builder.Configuration.GetSection("AppSettings"));

builder.Services.AddTransient<TokenHandler>();
builder.Services.AddHttpClient("ApiClient").AddHttpMessageHandler<TokenHandler>();

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

builder.Services.AddAuthentication("AppCookieAuth")
    .AddCookie("AppCookieAuth", options =>
    {
        options.Cookie.Name = "JPApp.Auth";
        options.LoginPath = "/Auth/Login";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    });

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ConfigureHttpsDefaults(httpsOptions =>
    {
        httpsOptions.OnAuthenticate = (context, authOptions) =>
        {
            var cert = authOptions.ServerCertificate;

            if (cert != null)
            {
                Console.WriteLine($"Certificate Subject: {cert.Subject}");

                if (cert is X509Certificate2 cert2)
                {
                    Console.WriteLine($"Certificate Thumbprint: {cert2.Thumbprint}");
                    Console.WriteLine($"Certificate Expiry: {cert2.NotAfter}");
                }
                else
                {
                    Console.WriteLine("Certificate does not support thumbprint or expiry information.");
                }
            }
            else
            {
                Console.WriteLine("No certificate loaded.");
            }
        };
    });
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
