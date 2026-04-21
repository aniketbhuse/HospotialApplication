using HealthcareApp.Data;
using HealthcareApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace HealthcareApp.Controllers
{
    public class DoctorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DoctorController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            try
            {
                var role = HttpContext.Session.GetString("UserRoleId");

                if (role != "2")
                {
                    return RedirectToAction("Index", "Home");
                }

                return RedirectToAction("Appointments");
            }
            catch (Exception ex)
            {
                return Content("Error: " + ex.Message);
            }
        }

        public IActionResult Appointments()
        {
            try
            {
                var userIdStr = HttpContext.Session.GetString("UserRoleId");

                if (string.IsNullOrEmpty(userIdStr))
                    return RedirectToAction("Login", "Account");

                int doctorUserId = Convert.ToInt32(userIdStr);

                var doctor = _context.Doctors
                    .FirstOrDefault(d => d.UserId == doctorUserId);

                if (doctor == null)
                    return Content("Doctor not found");

                var data = (from a in _context.Appointments
                            join p in _context.Patients on a.PatientId equals p.PatientId
                            join u in _context.Users on p.UserId equals u.UserId
                            where a.DoctorId == doctor.DoctorId
                            select new DoctorAppointmentVM
                            {
                                AppointmentId = a.AppointmentId,
                                AppointmentDate = a.AppointmentDate,
                                Time = a.Time,
                                Status = a.Status,

                                PatientName = u.FullName,
                                Age = p.Age,
                                Gender = p.Gender,
                                Contact = p.Contact,
                                Description = p.Description
                            }).ToList();

                return View(data);
            }
            catch (Exception ex)
            {
                return Content("Error: " + ex.Message);
            }
        }

        [HttpPost]
        public IActionResult UpdateStatus(int id, int status)
        {
            try
            {
                var appt = _context.Appointments.Find(id);

                if (appt == null)
                    return Content("Appointment not found");

                appt.Status = status;
                _context.SaveChanges();

                return RedirectToAction("Appointments");
            }
            catch (Exception ex)
            {
                return Content("Error updating status: " + ex.Message);
            }
        }
    }
}

