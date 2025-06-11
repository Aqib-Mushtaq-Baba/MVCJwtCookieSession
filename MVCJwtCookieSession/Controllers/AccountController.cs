using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MVCJwtCookieSession.Models;
using MVCJwtCookieSession.Services;

namespace MVCJwtCookieSession.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly JwtService _jwtService;

        public AccountController(UserManager<ApplicationUser> userManager,
                                 SignInManager<ApplicationUser> signInManager,
                                 JwtService jwtService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(string email, string password)
        {
            var user = new ApplicationUser { UserName = email, Email = email };
            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
                return RedirectToAction("Login");

            ViewBag.Errors = result.Errors;
            return View();
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                ViewBag.Message = "Invalid credentials";
                return View();
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName, password, false, false);
            if (result.Succeeded)
            {
                var token = _jwtService.GenerateJwtToken(user);

                // Store token in HTTP-Only cookie
                Response.Cookies.Append("jwt_token", token, new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTimeOffset.Now.AddHours(1)
                });

                // Store in Session too (not mandatory, just to learn) usually stores on server
                HttpContext.Session.SetString("jwt_token", token);

                return RedirectToAction("Index", "Employee");
            }

            ViewBag.Message = "Login failed";
            return View();
        }

        public IActionResult Logout()
        {
            Response.Cookies.Delete("jwt_token");
            HttpContext.Session.Remove("jwt_token");
            return RedirectToAction("Login");
        }
    }
}
