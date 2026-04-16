using Microsoft.AspNetCore.Mvc;
using HealthcareApp.Models;
using System;
using HealthcareApp.Data;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace HealthcareApp.Controllers
{
    
    public class CreateAccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CreateAccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            // Prefill CreatedDate so the form shows a reasonable default
            var model = new User { CreatedDate = DateTime.UtcNow };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(User model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var existingUser = _context.Users.FirstOrDefault(u => u.Email == model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "Email already exists");
                return View(model);
            }
            // Ensure timestamps
            if (model.CreatedDate == default)
            {
                model.CreatedDate = DateTime.UtcNow;
            }
            model.UpdatedDate = DateTime.UtcNow;
            model.RoleId = 3;

            _context.Users.Add(model);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Account created successfully!";

            return RedirectToAction("Login", "Account");
        }

    }
}
