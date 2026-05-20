using HealthcareApp.Data;
using HealthcareApp.Models;
using HealthcareApp.Models.Enums;
using Microsoft.AspNetCore.Mvc;

namespace HealthcareApp.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            try
            {
                var userEmail = HttpContext.Session.GetString("UserEmail");

                if (string.IsNullOrEmpty(userEmail))
                {
                    return RedirectToAction("Login", "Account");
                }

                var dbUser = _context.Users
                    .FirstOrDefault(u => u.Email == userEmail);

                if (dbUser == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var model = new PatientProfileViewModel
                {
                    FullName = dbUser.FullName,
                    Email = dbUser.Email,
                    Doctors = _context.Doctors.ToList(),
                    Appointments = new List<AppointmentDetailsViewModel>()
                };

                var patient = _context.Patients
                    .FirstOrDefault(p => p.UserId == dbUser.UserId);

                if (patient != null)
                {
                    model.Appointments = _context.Appointments
                        .Where(a => a.PatientId == patient.PatientId)
                        .Select(a => new AppointmentDetailsViewModel
                        {
                            AppointmentId = a.AppointmentId,
                            AppointmentDate = a.AppointmentDate,
                            Time = a.Time,
                            Status = a.Status,

                            DoctorName = _context.Doctors
                                .Where(d => d.DoctorId == a.DoctorId)
                                .Select(d => d.FullName)
                                .FirstOrDefault(),

                            PatientName = dbUser.FullName,
                            Age = patient.Age,
                            Gender = patient.Gender,
                            Contact = patient.Contact,
                            Description = patient.Description
                        })
                        .OrderByDescending(a => a.AppointmentDate)
                        .Distinct()
                        .ToList();
                }

                return View(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                TempData["ErrorMessage"] =
                    "Something went wrong while loading dashboard.";

                return RedirectToAction("Login", "Account");
            }
        }

        [HttpPost]
        public IActionResult BookAppointment(
            int DoctorId,
            DateTime AppointmentDate,
            TimeSpan Time,
            int Age,
            string Gender,
            string Contact,
            string Description)
        {
            try
            {
                var userEmail = HttpContext.Session.GetString("UserEmail");

                if (string.IsNullOrEmpty(userEmail))
                {
                    return RedirectToAction("Login", "Account");
                }

                var user = _context.Users
                    .FirstOrDefault(u => u.Email == userEmail);

                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                // CHECK IF PATIENT EXISTS
                var patient = _context.Patients
                    .FirstOrDefault(p => p.UserId == user.UserId);

                if (patient == null)
                {
                    patient = new Patient
                    {
                        UserId = user.UserId,
                        Age = Age,
                        Gender = Gender,
                        Contact = Contact,
                        Description = Description,
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now
                    };

                    _context.Patients.Add(patient);
                    _context.SaveChanges();
                }
                else
                {
                    patient.Age = Age;
                    patient.Gender = Gender;
                    patient.Contact = Contact;
                    patient.Description = Description;
                    patient.UpdatedDate = DateTime.Now;

                    _context.Patients.Update(patient);
                    _context.SaveChanges();
                }

                // PREVENT DUPLICATE APPOINTMENTS
                var alreadyExists = _context.Appointments.Any(a =>
                    a.PatientId == patient.PatientId &&
                    a.DoctorId == DoctorId &&
                    a.AppointmentDate == AppointmentDate &&
                    a.Time == Time);

                if (alreadyExists)
                {
                    TempData["ErrorMessage"] =
                        "Appointment already exists.";

                    return RedirectToAction("Index");
                }

                // SAVE APPOINTMENT
                var appointment = new Appointments
                {
                    DoctorId = DoctorId,
                    PatientId = patient.PatientId,
                    AppointmentDate = AppointmentDate,
                    Time = Time,
                    Status = (int)AppointmentStatus.Pending,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };

                _context.Appointments.Add(appointment);
                _context.SaveChanges();

                TempData["SuccessMessage"] =
                    "Appointment booked successfully!";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                TempData["ErrorMessage"] =
                    "Something went wrong while booking appointment.";
            }

            return RedirectToAction("Index");
        }
    }
}