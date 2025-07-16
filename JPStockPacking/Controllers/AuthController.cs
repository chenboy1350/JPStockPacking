using JPStockPacking.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace JPStockPacking.Controllers
{
    public class AuthController(IAuthService userService) : Controller
    {
        private readonly IAuthService _authService = userService;

        public IActionResult Login()
        {
            var rememberedUsername = Request.Cookies["RememberedUsername"];
            var rememberMeChecked = Request.Cookies["RememberMeChecked"] == "true";

            ViewBag.RememberedUsername = rememberedUsername;
            ViewBag.RememberMeChecked = rememberMeChecked;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password, bool remember)
        {
            var result = await _authService.LoginUserAsync(username, password, remember);

            if (!result.Success)
                return Json(new { success = false, message = result.Message });

            return Json(new { success = true, redirectUrl = Url.Action("Index", "Home") });
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync();
            return Json(new
            {
                success = true,
                redirectUrl = Url.Action("Login", "Auth")
            });
        }

        [HttpPost("refresh-session")]
        public async Task<IActionResult> RefreshSession()
        {
            var result = await _authService.RefreshTokenAsync();

            if (result.Success)
            {
                return Ok(new { message = "Session refreshed successfully" });
            }

            return Unauthorized(new { message = result.Message });
        }

        [HttpPost("logout-session")]
        public async Task<IActionResult> LogoutSession()
        {
            await _authService.LogoutAsync();
            return Ok(new { message = "Logged out successfully" });
        }
    }
}
