using Microsoft.AspNetCore.Mvc;

namespace HealthcareApp.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
