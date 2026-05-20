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
                var role =
                    HttpContext.Session.GetString("UserRoleId");

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
                // GET LOGGED-IN USER ID
                var userIdStr =
                    HttpContext.Session.GetString("UserId");

                if (string.IsNullOrEmpty(userIdStr))
                {
                    return RedirectToAction("Login", "Account");
                }

                int userId = Convert.ToInt32(userIdStr);

                // FIND DOCTOR USING USERID
                var doctor = _context.Doctors
                    .FirstOrDefault(d => d.UserId == userId);

                if (doctor == null)
                {
                    return Content("Doctor not found");
                }

                // GET APPOINTMENTS FOR THIS DOCTOR
                var data = (from a in _context.Appointments

                            join p in _context.Patients
                            on a.PatientId equals p.PatientId

                            join u in _context.Users
                            on p.UserId equals u.UserId

                            where a.DoctorId == doctor.DoctorId

                            orderby a.AppointmentDate descending

                            select new DoctorAppointmentVM
                            {
                                AppointmentId = a.AppointmentId,

                                AppointmentDate =
                                    a.AppointmentDate,

                                Time = a.Time,

                                Status = a.Status,

                                PatientName = u.FullName,

                                //Age = a.Age,

                                //Gender = a.Gender,

                                //Contact = a.Contact,

                                //Description = a.Description
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
                var appointment =
                    _context.Appointments
                    .FirstOrDefault(a => a.AppointmentId == id);

                if (appointment == null)
                {
                    return Content("Appointment not found");
                }

                appointment.Status = status;

                appointment.UpdatedDate = DateTime.Now;

                _context.SaveChanges();

                TempData["SuccessMessage"] =
                    "Appointment status updated successfully.";

                return RedirectToAction("Appointments");
            }
            catch (Exception ex)
            {
                return Content("Error updating status: " + ex.Message);
            }
        }
    }
}