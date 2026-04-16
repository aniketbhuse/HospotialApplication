using Microsoft.AspNetCore.Mvc;

namespace HealthcareApp.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            var roleId = HttpContext.Session.GetString("UserRoleId");

            // 🔐 Only Admin allowed
            if (roleId != "1")
            {
                return RedirectToAction("Index", "Admin");
            }

            return View();
        }
    }
}
