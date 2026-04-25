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

                var user = _context.Users
                    .Where(u => u.Email == userEmail)
                    .Select(u => new PatientProfileViewModel
                    {
                        FullName = u.FullName,
                        Email = u.Email,
                        Doctors = _context.Doctors.ToList()
                    })
                    .FirstOrDefault();

                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                // ✅ ADD THIS PART (Appointments Fetch Logic)
                var dbUser = _context.Users.FirstOrDefault(u => u.Email == userEmail);

                if (dbUser != null)
                {
                    var patient = _context.Patients
                        .FirstOrDefault(p => p.UserId == dbUser.UserId);

                    if (patient != null)
                    {
                        user.Appointments = _context.Appointments
                            .Where(a => a.PatientId == patient.PatientId)
                            .Join(_context.Doctors,
                                a => a.DoctorId,
                                d => d.DoctorId,
                                (a, d) => new AppointmentDetailsViewModel
                                {
                                    AppointmentId = a.AppointmentId,
                                    AppointmentDate = a.AppointmentDate,
                                    Time = a.Time,
                                    Status = a.Status,

                                    DoctorName = d.FullName,

                                    PatientName = dbUser.FullName,
                                    Age = patient.Age,
                                    Gender = patient.Gender,
                                    Contact = patient.Contact,
                                    Description = patient.Description
                                }).ToList();
                    }
                }

                return View(user);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                TempData["ErrorMessage"] = "Something went wrong while loading dashboard.";

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

                var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);

                if (user == null)
                    return RedirectToAction("Login", "Account");

                // ✅ Check if patient exists
                var existingPatient = _context.Patients
                    .FirstOrDefault(p => p.UserId == user.UserId);

                Patient patient;

                if (existingPatient != null)
                {
                    patient = existingPatient;

                    patient.Age = Age;
                    patient.Gender = Gender;
                    patient.Contact = Contact;
                    patient.Description = Description;
                    patient.UpdatedDate = DateTime.Now;

                    _context.Patients.Update(patient);
                    _context.SaveChanges();
                }
                else
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

                // ✅ Save Appointment
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

                TempData["SuccessMessage"] = "Appointment created successfully!";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                TempData["ErrorMessage"] = "Something went wrong while booking appointment.";
            }

            return RedirectToAction("Index");
        }
    }
}

