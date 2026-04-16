using Microsoft.AspNetCore.Mvc;

namespace HealthcareApp.Controllers
{
    public class DoctorController : Controller
    {
        public IActionResult Index()
        {
            // 🔐 Restrict access
            var role = HttpContext.Session.GetString("UserRoleId");

            if (role != "2")
            {
                return RedirectToAction("Index", "Doctor");
            }

            return View();
        }
    }
}
