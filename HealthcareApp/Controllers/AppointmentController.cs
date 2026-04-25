using HealthcareApp.Data;
using HealthcareApp.Models;
using HealthcareApp.Models.Enums;
using Microsoft.AspNetCore.Mvc;

namespace HealthcareApp.Controllers
{
    public class AppointmentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AppointmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 🔹 Load Partial View
        public IActionResult BookAppointmentPartial()
        {

            return PartialView("_BookAppointment");
            /* var doctors = _context.Doctors.ToList();
             var model = new BookAppointmentViewModel
             {
                 Doctors = doctors
             };
             return PartialView("~/Views/Shared/_BookAppointment.cshtml", model);*/
        }

        // 🔹 Save Appointment
     /*   [HttpPost]
        public IActionResult BookAppointment(BookAppointmentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Doctors = _context.Doctors.ToList();
                return PartialView("_BookAppointment", model);
            }

            var userEmail = HttpContext.Session.GetString("UserEmail");

            var patient = _context.Users.FirstOrDefault(u => u.Email == userEmail);

            var appointment = new Appointments
            {
                DoctorId = model.DoctorId,
                PatientId = patient.UserId,
                AppointmentDate = model.AppointmentDate,
                Status = (int)AppointmentStatus.Pending,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            };

            _context.Appointments.Add(appointment);
            _context.SaveChanges();

            return Json(new { success = true });
        }*/
    }
}
