using CarRentalApplication_API.Services;
using HealthcareApp.Data;
using HealthcareApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

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

