using Microsoft.AspNetCore.Mvc;
using HealthcareApp.Models;
using HealthcareApp.Data;

namespace HealthcareApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly ApplicationDbContext _context;

        public AccountController(ILogger<AccountController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // GET
        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            var model = new LoginViewModel { ReturnUrl = returnUrl };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = _context.Users.FirstOrDefault(u =>
                u.Email.Trim().ToLower() == model.Username.Trim().ToLower()
                && u.Password == model.Password);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid username or password");
                return View(model);
            }

            // Session set
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserName", user.FullName);
            HttpContext.Session.SetString("UserRoleId", user.RoleId.ToString());

            Console.WriteLine("Login Success: " + user.Email);

            // ✅ Redirect
            if (user.RoleId == 2)
                return RedirectToAction("Index", "Doctor");

            else if (user.RoleId == 1)
                return RedirectToAction("Index", "Admin");

            else
                return RedirectToAction("Index", "Dashboard");
        }

        // Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // Protected page example
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("UserEmail") == null)
            {
                return RedirectToAction("Login");
            }

            return View();
        }
    }
}