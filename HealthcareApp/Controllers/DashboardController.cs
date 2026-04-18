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

//Demo Code 


using auto.ecom.api.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace auto.ecom.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChildCategoryController : Controller
    {
        private readonly AutoDbContext _context;

        public ChildCategoryController(AutoDbContext context)
        {
            _context = context;
        }

        [HttpGet("getAllChildCategory")]
        public IActionResult GetChildCategories()
        {
            var categories = (from ccm in _context.ChildCategoryMasters
                              join cm in _context.CategoryMasters
                              on ccm.categoryId equals cm.categoryId
                              select new
                              {
                                  ccm.categoryId,
                                  ccm.childCategoryName,
                                  cm.categoryName
                              }).ToList();
            // var categories = _context.ChildCategoryMasters.ToList();
            return Ok(categories);
        }

        [HttpPost("AddChildCategory")]
        public IActionResult AddChildCategory([FromBody] ChildCategoryMaster obj)
        {
            if (obj == null)
                return BadRequest("Invalid data!");

            //  Check for duplicate record (same name under the same category)
            var isExist = _context.ChildCategoryMasters
                .Any(x => x.childCategoryName.ToLower() == obj.childCategoryName.ToLower()
                       && x.categoryId == obj.categoryId);

            if (isExist)
                return Conflict("Duplicate Entry! This child category already exists under the same category.");

            //  Add record to DB
            _context.ChildCategoryMasters.Add(obj);
            _context.SaveChanges();

            return Ok("Child Category Added Successfully");
        }

        [HttpPut("UpdateChildCategory")]
        public IActionResult UpdateChildCategory([FromBody] ChildCategoryMaster obj)
        {
            // Find record using primary key
            var existingChild = _context.ChildCategoryMasters.FirstOrDefault(x => x.childCategoryId == obj.childCategoryId);

            if (existingChild == null)
            {
                return NotFound("Child Category not found!");
            }

            // Update fields
            existingChild.childCategoryName = obj.childCategoryName;
            existingChild.categoryId = obj.categoryId;

            // Save to DB
            _context.SaveChanges();

            return Ok("Child Category Updated Successfully");
        }

        [HttpDelete("DeleteChildCategory/{id}")]
        public IActionResult DeleteChildCategory(int id)
        {
            // Find record by primary key
            var childCategory = _context.ChildCategoryMasters.FirstOrDefault(x => x.childCategoryId == id);

            if (childCategory == null)
            {
                return NotFound("Child Category not found!");
            }

            // Remove from database
            _context.ChildCategoryMasters.Remove(childCategory);
            _context.SaveChanges();

            return Ok("Child Category deleted successfully");
        }


    }
}


using auto.ecom.api.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace auto.ecom.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : Controller
    {
        private readonly AutoDbContext _context;

        public CustomerController(AutoDbContext context)
        {
            _context = context;
        }

        //  GET ALL CUSTOMERS
        [HttpGet("GetAll")]
        public IActionResult GetAllCustomers()
        {
            try
            {
                var customers = _context.Customers.ToList();
                return Ok(customers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        //  GET CUSTOMER BY ID
        [HttpGet("GetById/{id}")]
        public IActionResult GetCustomerById(int id)
        {
            try
            {
                var customer = _context.Customers.Find(id);

                if (customer == null)
                    return NotFound("Customer not found!");

                return Ok(customer);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpPost("Add")]
        public async Task<IActionResult> AddCustomer([FromBody] Customer obj)
        {
            try
            {
                if (obj == null)
                    return BadRequest("Invalid data!");

                //  Duplicate Check: Same Name AND Same Mobile No shouldn't exist
                bool exists = await _context.Customers
                    .AnyAsync(x => x.customerName.Trim().ToLower() == obj.customerName.Trim().ToLower()
                                && x.mobileNo.Trim() == obj.mobileNo.Trim());

                if (exists)
                    return Conflict("Customer with the same Name and Mobile Number already exists!");

                //  Save new customer
                await _context.Customers.AddAsync(obj);
                await _context.SaveChangesAsync();

                return Ok("Customer added successfully!");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        //  PUT - UPDATE CUSTOMER BY ID
        [HttpPut("UpdateCustomer/{id}")]
        public IActionResult UpdateCustomer(int id, [FromBody] Customer obj)
        {
            try
            {
                var existingCustomer = _context.Customers.Find(id);

                if (existingCustomer == null)
                    return NotFound("Customer not found!");

                //  Update fields
                existingCustomer.customerName = obj.customerName;
                existingCustomer.mobileNo = obj.mobileNo;
                existingCustomer.email = obj.email;
                existingCustomer.city = obj.city;
                existingCustomer.address = obj.address;
                existingCustomer.pincode = obj.pincode;

                _context.SaveChanges();
                return Ok("Customer updated successfully!");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        //  DELETE CUSTOMER BY ID
        [HttpDelete("DeleteCustomer/{id}")]
        public IActionResult DeleteCustomer(int id)
        {
            try
            {
                var customer = _context.Customers.Find(id);

                if (customer == null)
                    return NotFound("Customer not found!");

                _context.Customers.Remove(customer);
                _context.SaveChanges();

                return Ok("Customer deleted successfully!");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
    }
}
