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

// Demo Code

using CarRentalApplication_API.Model;
using Microsoft.EntityFrameworkCore;

namespace CarRentalApplication_API.Services
{
    public class PriceService : IPriceService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogService _logService;

        public PriceService(ApplicationDbContext context, ILogService logService)
        {
            _context = context;
            _logService = logService;
        }

        public async Task<List<Price>> GetAllPricesAsync()
        {
            try
            {
                var prices = await _context.Prices.Include(p => p.Vehicle).Include(p => p.VehicleCategory).ToListAsync();
                await _logService.AddLogAsync("Retrieved all prices", "Info");
                return prices;
            }
            catch (Exception ex)
            {
                await _logService.AddLogAsync($"Error retrieving prices: {ex.Message}", "Error");
                throw;
            }
        }

        public async Task<Price?> GetPriceByIdAsync(int id)
        {
            try
            {
                var price = await _context.Prices.FindAsync(id);
                if (price == null)
                {
                    await _logService.AddLogAsync($"Price id {id} not found", "Warning");
                    return null;
                }
                await _logService.AddLogAsync($"Retrieved price {id}", "Info");
                return price;
            }
            catch (Exception ex)
            {
                await _logService.AddLogAsync($"Error retrieving price id {id}: {ex.Message}", "Error");
                throw;
            }
        }

        public async Task<Price> AddPriceAsync(Price price)
        {
            try
            {
                price.createdAt = DateTime.UtcNow;
                _context.Prices.Add(price);
                await _context.SaveChangesAsync();
                await _logService.AddLogAsync($"Added price id {price.price_id}", "Info");
                return price;
            }
            catch (Exception ex)
            {
                await _logService.AddLogAsync($"Error adding price: {ex.Message}", "Error");
                throw;
            }
        }

        public async Task<Price?> UpdatePriceAsync(int id, Price updatedPrice)
        {
            try
            {
                var price = await _context.Prices.FindAsync(id);
                if (price == null)
                {
                    await _logService.AddLogAsync($"Price id {id} not found for update", "Warning");
                    return null;
                }

                price.vehicle_Id = updatedPrice.vehicle_Id;
                price.category_id = updatedPrice.category_id;
                price.base_price_per_day = updatedPrice.base_price_per_day;
                price.weekend_price = updatedPrice.weekend_price;
                price.holiday_price = updatedPrice.holiday_price;
                price.discount_percentage = updatedPrice.discount_percentage;
                price.effective_from = updatedPrice.effective_from;
                price.effective_to = updatedPrice.effective_to;
                price.isActive = updatedPrice.isActive;
                price.updatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await _logService.AddLogAsync($"Updated price id {id}", "Info");
                return price;
            }
            catch (Exception ex)
            {
                await _logService.AddLogAsync($"Error updating price id {id}: {ex.Message}", "Error");
                throw;
            }
        }

        public async Task<bool> DeletePriceAsync(int id)
        {
            try
            {
                var price = await _context.Prices.FindAsync(id);
                if (price == null)
                {
                    await _logService.AddLogAsync($"Price id {id} not found for delete", "Warning");
                    return false;
                }
                _context.Prices.Remove(price);
                await _context.SaveChangesAsync();
                await _logService.AddLogAsync($"Deleted price id {id}", "Warning");
                return true;
            }
            catch (Exception ex)
            {
                await _logService.AddLogAsync($"Error deleting price id {id}: {ex.Message}", "Error");
                throw;
            }
        }

        public async Task<List<Price>> GetPricesByVehicleIdAsync(int vehicleId)
        {
            try
            {
                var prices = await _context.Prices.Where(p => p.vehicle_Id == vehicleId).ToListAsync();
                await _logService.AddLogAsync($"Retrieved prices for vehicle {vehicleId}", "Info");
                return prices;
            }
            catch (Exception ex)
            {
                await _logService.AddLogAsync($"Error retrieving prices for vehicle {vehicleId}: {ex.Message}", "Error");
                throw;
            }
        }

        public async Task<List<Price>> GetActivePricesAsync()
        {
            try
            {
                var prices = await _context.Prices.Where(p => p.isActive).ToListAsync();
                await _logService.AddLogAsync("Retrieved active prices", "Info");
                return prices;
            }
            catch (Exception ex)
            {
                await _logService.AddLogAsync($"Error retrieving active prices: {ex.Message}", "Error");
                throw;
            }
        }
    }
}


using CarRentalApplication_API.Model;
using Microsoft.EntityFrameworkCore;

namespace CarRentalApplication_API.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogService _logService;

        public VehicleService(ApplicationDbContext context, ILogService logService)
        {
            _context = context;
            _logService = logService;
        }

        public async Task<List<VehicleDto>> GetAllVehiclesAsync()
        {
            try
            {
                var vehicles = await (from v in _context.Vehicles
                                      join c in _context.vehicle_Categories on v.category_id equals c.category_id
                                      select new VehicleDto
                                      {

                                          vehicleName = v.vehicleName,
                                          vehicleModel = v.vehicleModel,
                                          seating_capacity = v.seating_capacity,
                                          price_per_day = v.price_per_day,
                                          qunatity = v.qunatity,
                                          seats = v.seats,
                                          status = v.status,
                                          transmission_Type = v.transmission_Type,
                                          license_Plate = v.license_Plate,
                                          category_Name = c.category_Name,
                                          Description = c.Description
                                      }).ToListAsync();

                await _logService.AddLogAsync("Retrieved all vehicles successfully", "Info");
                return vehicles;
            }
            catch (Exception ex)
            {
                await _logService.AddLogAsync($"Error retrieving vehicles: {ex.Message}", "Error");
                throw;
            }
        }

        public async Task<VehicleDto> GetVehicleByIdAsync(int id)
        {
            try
            {
                var vehicle = await (from v in _context.Vehicles
                                     join c in _context.vehicle_Categories on v.category_id equals c.category_id
                                     where v.vehicle_Id == id
                                     select new VehicleDto
                                     {
                                         vehicle_Id = v.vehicle_Id,
                                         vehicleName = v.vehicleName,
                                         vehicleModel = v.vehicleModel,
                                         seating_capacity = v.seating_capacity,
                                         price_per_day = v.price_per_day,
                                         qunatity = v.qunatity,
                                         seats = v.seats,
                                         status = v.status,
                                         transmission_Type = v.transmission_Type,
                                         license_Plate = v.license_Plate,
                                         createdAt = v.createdAt,
                                         updatedAt = v.updatedAt,
                                         category_id = v.category_id,
                                         category_Name = c.category_Name,
                                         Description = c.Description
                                     }).FirstOrDefaultAsync();

                if (vehicle == null)
                {
                    await _logService.AddLogAsync($"Vehicle with ID {id} not found", "Warning");
                    return null;
                }

                await _logService.AddLogAsync($"Retrieved vehicle with ID {id}", "Info");
                return vehicle;
            }
            catch (Exception ex)
            {
                await _logService.AddLogAsync($"Error retrieving vehicle ID {id}: {ex.Message}", "Error");
                throw;
            }
        }

        public async Task<Vehicles> AddVehicleAsync(Vehicles vehicle)
        {
            try
            {
                vehicle.createdAt = DateTime.UtcNow;

                _context.Vehicles.Add(vehicle);
                await _context.SaveChangesAsync();

                await _logService.AddLogAsync($"Added vehicle with ID {vehicle.vehicle_Id}", "Info");

                return vehicle;
            }
            catch (Exception ex)
            {
                await _logService.AddLogAsync($"Error adding vehicle: {ex.Message}", "Error");
                throw;
            }
        }

        public async Task<Vehicles> UpdateVehicleAsync(int id, Vehicles updatedVehicle)
        {
            try
            {
                var vehicle = await _context.Vehicles.FindAsync(id);

                if (vehicle == null)
                {
                    await _logService.AddLogAsync($"Vehicle ID {id} not found for update", "Warning");
                    return null;
                }

                vehicle.vehicleName = updatedVehicle.vehicleName;
                vehicle.vehicleModel = updatedVehicle.vehicleModel;
                vehicle.seating_capacity = updatedVehicle.seating_capacity;
                vehicle.price_per_day = updatedVehicle.price_per_day;
                vehicle.qunatity = updatedVehicle.qunatity;
                vehicle.seats = updatedVehicle.seats;
                vehicle.status = updatedVehicle.status;
                vehicle.transmission_Type = updatedVehicle.transmission_Type;
                vehicle.license_Plate = updatedVehicle.license_Plate;
                vehicle.category_id = updatedVehicle.category_id;
                vehicle.updatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                await _logService.AddLogAsync($"Updated vehicle with ID {id}", "Info");

                return vehicle;
            }
            catch (Exception ex)
            {
                await _logService.AddLogAsync($"Error updating vehicle ID {id}: {ex.Message}", "Error");
                throw;
            }
        }

        public async Task<bool> DeleteVehicleAsync(int id)
        {
            try
            {
                var vehicle = await _context.Vehicles.FindAsync(id);

                if (vehicle == null)
                {
                    await _logService.AddLogAsync($"Vehicle ID {id} not found for deletion", "Warning");
                    return false;
                }

                _context.Vehicles.Remove(vehicle);
                await _context.SaveChangesAsync();

                await _logService.AddLogAsync($"Deleted vehicle with ID {id}", "Warning");

                return true;
            }
            catch (Exception ex)
            {
                await _logService.AddLogAsync($"Error deleting vehicle ID {id}: {ex.Message}", "Error");
                throw;
            }
        }

        public async Task<Vehicles> UpdateVehicleStatusAsync(int id, string status)
        {
            try
            {
                var vehicle = await _context.Vehicles.FindAsync(id);

                if (vehicle == null)
                {
                    await _logService.AddLogAsync($"Vehicle ID {id} not found for status update", "Warning");
                    return null;
                }

                vehicle.status = status;
                vehicle.updatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                await _logService.AddLogAsync($"Updated vehicle ID {id} status to {status}", "Info");

                return vehicle;
            }
            catch (Exception ex)
            {
                await _logService.AddLogAsync($"Error updating status for vehicle ID {id}: {ex.Message}", "Error");
                throw;
            }
        }

        /* This method retrieves all vehicle categories from the database. 
         * It uses Entity Framework Core to asynchronously fetch the list of categories and logs the operation.
         * If an error occurs during the retrieval process, it logs the error and rethrows the exception to be handled by the calling code. */

        public async Task<List<vehicle_categories>> GetAllVehicle_CategoriesAsync()
        {
            try
            {
                var categories = await _context.vehicle_Categories.ToListAsync();
                await _logService.AddLogAsync("Retrieved all vehicle categories successfully", "Info");
                return categories;
            }
            catch (Exception ex)
            {
                await _logService.AddLogAsync($"Error retrieving vehicle categories: {ex.Message}", "Error");
                throw;
            }
        }

        public async Task<vehicle_categories> GetVehicle_CategoryByIdAsync(int id)
        {
            try
            {
                var category = await _context.vehicle_Categories.FindAsync(id);
                if (category == null)
                {
                    await _logService.AddLogAsync($"Vehicle category with ID {id} not found", "Warning");
                    return null;
                }
                await _logService.AddLogAsync($"Retrieved vehicle category with ID {id}", "Info");
                return category;
            }
            catch (Exception ex)
            {
                await _logService.AddLogAsync($"Error retrieving vehicle category ID {id}: {ex.Message}", "Error");
                throw;
            }

        }

        public async Task<vehicle_categories> AddVehicle_CategoryAsync(vehicle_categories category)
        {
            try
            {
                _context.vehicle_Categories.Add(category);
                await _context.SaveChangesAsync();
                await _logService.AddLogAsync($"Added vehicle category with ID {category.category_id}", "Info");
                return category;
            }
            catch (Exception ex)
            {
                await _logService.AddLogAsync($"Error adding vehicle category: {ex.Message}", "Error");
                throw;
            }
        }

        public async Task<vehicle_categories> UpdateVehicle_CategoryAsync(int id, vehicle_categories updatedCategory)
        {
            try
            {
                var category = await _context.vehicle_Categories.FindAsync(id);
                if (category == null)
                {
                    await _logService.AddLogAsync($"Vehicle category ID {id} not found for update", "Warning");
                    return null;
                }
                category.category_Name = updatedCategory.category_Name;
                category.Description = updatedCategory.Description;
                await _context.SaveChangesAsync();
                await _logService.AddLogAsync($"Updated vehicle category with ID {id}", "Info");
                return category;
            }
            catch (Exception ex)
            {
                await _logService.AddLogAsync($"Error updating vehicle category ID {id}: {ex.Message}", "Error");
                throw;
            }
        }

        public async Task<bool> DeleteVehicle_CategoryAsync(int id)
        {
            try
            {
                var category = await _context.vehicle_Categories.FindAsync(id);
                if (category == null)
                {
                    await _logService.AddLogAsync($"Vehicle category ID {id} not found for deletion", "Warning");
                    return false;
                }
                _context.vehicle_Categories.Remove(category);
                await _context.SaveChangesAsync();
                await _logService.AddLogAsync($"Deleted vehicle category with ID {id}", "Warning");
                return true;
            }
            catch (Exception ex)
            {
                await _logService.AddLogAsync($"Error deleting vehicle category ID {id}: {ex.Message}", "Error");
                throw;
            }
        }
    }
}

using CarRentalApplication_API.Model;
using CarRentalApplication_API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace CarRentalApplication_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogService _logService;

        public BookingController(ApplicationDbContext context, ILogService logService)
        {
            _context = context;
            _logService = logService;
        }

        // ========================= GET ALL BOOKINGS =========================
        [HttpGet("GetAllBooking")]
        public async Task<IActionResult> GetBooking()
        {
            try
            {
                var booking = await (from b in _context.Bookings
                                     join u in _context.Users on b.User_Id equals u.user_id
                                     join v in _context.Vehicles on b.Vehicle_Id equals v.vehicle_Id
                                     select new
                                     {
                                         u.first_Name,
                                         u.last_Name,
                                         u.email,
                                         u.phone_Number,
                                         b.Pickup_Datetime,
                                         b.Dropoff_Datetime,
                                         b.Total_Days,
                                         b.Price_Per_Day,
                                         b.Total_Amount,
                                         b.Booking_Status,
                                         v.vehicleName,
                                         v.vehicleModel,
                                         v.seating_capacity,
                                         v.license_Plate
                                     }).ToListAsync();

                await _logService.AddLogAsync("Retrieved all bookings successfully", "Info");

                return Ok(booking);
            }
            catch (Exception ex)
            {
                await _logService.AddLogAsync($"Error retrieving bookings: {ex.Message}", "Error");

                return StatusCode(500, new
                {
                    Status = "Error",
                    Message = "An error occurred while retrieving bookings",
                    Error = ex.Message
                });
            }
        }

        // ========================= ADD BOOKING =========================
        [HttpPost("AddBooking")]
        public async Task<IActionResult> AddBooking([FromBody] Booking booking)
        {
            try
            {
                if (booking == null)
                {
                    await _logService.AddLogAsync("Attempted to add null booking", "Warning");

                    return BadRequest(new
                    {
                        Status = "Error",
                        Message = "Booking data is null"
                    });
                }

                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

                await _logService.AddLogAsync($"Booking added successfully (ID: {booking.Booking_Id})", "Info");

                return Ok(new
                {
                    Status = "Success",
                    Message = "Booking added successfully",
                    Data = booking
                });
            }
            catch (Exception ex)
            {
                await _logService.AddLogAsync($"Error adding booking: {ex.Message}", "Error");

                return StatusCode(500, new
                {
                    Status = "Error",
                    Message = "An error occurred while adding the booking",
                    Error = ex.Message
                });
            }
        }

        // ========================= DELETE BOOKING =========================
        [HttpDelete("DeleteBooking/{id}")]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            try
            {
                var booking = await _context.Bookings.FindAsync(id);

                if (booking == null)
                {
                    await _logService.AddLogAsync($"Booking not found for deletion (ID: {id})", "Warning");

                    return NotFound(new
                    {
                        Status = "Error",
                        Message = "Booking not found"
                    });
                }

                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync();

                await _logService.AddLogAsync($"Booking deleted successfully (ID: {id})", "Warning");

                return Ok(new
                {
                    Status = "Success",
                    Message = "Booking deleted successfully"
                });
            }
            catch (Exception ex)
            {
                await _logService.AddLogAsync($"Error deleting booking (ID: {id}): {ex.Message}", "Error");

                return StatusCode(500, new
                {
                    Status = "Error",
                    Message = "An error occurred while deleting the booking",
                    Error = ex.Message
                });
            }
        }

        // ========================= UPDATE BOOKING =========================
        [HttpPut("UpdateBooking/{id}")]
        public async Task<IActionResult> UpdateBooking(int id, [FromBody] Booking updatedBooking)
        {
            try
            {
                var booking = await _context.Bookings.FindAsync(id);

                if (booking == null)
                {
                    await _logService.AddLogAsync($"Booking not found for update (ID: {id})", "Warning");

                    return NotFound(new
                    {
                        Status = "Error",
                        Message = "Booking not found"
                    });
                }

                booking.Pickup_Datetime = updatedBooking.Pickup_Datetime;
                booking.Dropoff_Datetime = updatedBooking.Dropoff_Datetime;
                booking.Total_Days = updatedBooking.Total_Days;
                booking.Price_Per_Day = updatedBooking.Price_Per_Day;
                booking.Total_Amount = updatedBooking.Total_Amount;
                booking.Booking_Status = updatedBooking.Booking_Status;
                booking.UpdatedAt = DateTime.UtcNow;

                _context.Bookings.Update(booking);
                await _context.SaveChangesAsync();

                await _logService.AddLogAsync($"Booking updated successfully (ID: {id})", "Info");

                return Ok(new
                {
                    Status = "Success",
                    Message = "Booking updated successfully",
                    Data = booking
                });
            }
            catch (Exception ex)
            {
                await _logService.AddLogAsync($"Error updating booking (ID: {id}): {ex.Message}", "Error");

                return StatusCode(500, new
                {
                    Status = "Error",
                    Message = "An error occurred while updating the booking",
                    Error = ex.Message
                });
            }
        }

        // ========================= GET BOOKING BY ID =========================
        [HttpGet("GetBookingById/{id}")]
        public async Task<IActionResult> GetBookingById(int id)
        {
            try
            {
                var booking = await _context.Bookings.FindAsync(id);

                if (booking == null)
                {
                    await _logService.AddLogAsync($"Booking not found (ID: {id})", "Warning");

                    return NotFound(new
                    {
                        Status = "Error",
                        Message = "Booking not found"
                    });
                }

                await _logService.AddLogAsync($"Retrieved booking successfully (ID: {id})", "Info");

                return Ok(new
                {
                    Status = "Success",
                    Message = "Booking retrieved successfully",
                    Data = booking
                });
            }
            catch (Exception ex)
            {
                await _logService.AddLogAsync($"Error retrieving booking (ID: {id}): {ex.Message}", "Error");

                return StatusCode(500, new
                {
                    Status = "Error",
                    Message = "An error occurred while retrieving the booking",
                    Error = ex.Message
                });
            }
        }
    }
}

using CarRentalApplication_API.Services;
using CarRentalApplication_API.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRentalApplication_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogService _logService;

        public LoginController(ApplicationDbContext context, ILogService logService)
        {
            _context = context;
            _logService = logService;
        }

        // ========================= USER LOGIN =========================
        [HttpPost("UserLogin")]
        public async Task<IActionResult> UserLogin([FromBody] LoginViewModel loginViewModel)
        {
            try
            {
                if (loginViewModel == null ||
                    string.IsNullOrEmpty(loginViewModel.phone_Number) ||
                    string.IsNullOrEmpty(loginViewModel.Password))
                {
                    await _logService.AddLogAsync(
                        "Login attempt failed due to missing phone number or password",
                        "Warning");

                    return BadRequest(new
                    {
                        Status = "Error",
                        Message = "Phone number and password are required"
                    });
                }

                var user = await _context.Users
                    .FirstOrDefaultAsync(u =>
                        u.phone_Number == loginViewModel.phone_Number &&
                        u.password == loginViewModel.Password);

                if (user != null)
                {
                    await _logService.AddLogAsync(
                        $"User login successful (User ID: {user.user_id}, Phone: {user.phone_Number})",
                        "Info");

                    return Ok(new
                    {
                        Status = "Success",
                        Message = "Login successful",
                        Data = user
                    });
                }
                else
                {
                    await _logService.AddLogAsync(
                        $"Invalid login attempt (Phone: {loginViewModel.phone_Number})",
                        "Warning");

                    return Unauthorized(new
                    {
                        Status = "Error",
                        Message = "Invalid phone number or password"
                    });
                }
            }
            catch (Exception ex)
            {
                await _logService.AddLogAsync(
                    $"Login error: {ex.Message}",
                    "Error");

                return StatusCode(500, new
                {
                    Status = "Error",
                    Message = "An error occurred during login",
                    Error = ex.Message
                });
            }
        }
    }
}


using CarRentalApplication_API.Model;
using CarRentalApplication_API.Services;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalApplication_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PricesController : Controller
    {
        private readonly IPriceService _priceService;

        public PricesController(IPriceService priceService)
        {
            _priceService = priceService;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var prices = await _priceService.GetAllPricesAsync();
                return Ok(prices);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Status = "Error", Message = "Error retrieving prices", Error = ex.Message });
            }
        }

        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var price = await _priceService.GetPriceByIdAsync(id);
                if (price == null) return NotFound(new { Status = "Error", Message = "Price not found" });
                return Ok(price);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Status = "Error", Message = "Error retrieving price", Error = ex.Message });
            }
        }

        [HttpPost("Add")]
        public async Task<IActionResult> Add([FromBody] Price price)
        {
            try
            {
                var result = await _priceService.AddPriceAsync(price);
                return Ok(new { Status = "Success", Message = "Price added", Data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Status = "Error", Message = "Error adding price", Error = ex.Message });
            }
        }

        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Price updatedPrice)
        {
            try
            {
                var result = await _priceService.UpdatePriceAsync(id, updatedPrice);
                if (result == null) return NotFound(new { Status = "Error", Message = "Price not found" });
                return Ok(new { Status = "Success", Message = "Price updated", Data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Status = "Error", Message = "Error updating price", Error = ex.Message });
            }
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _priceService.DeletePriceAsync(id);
                if (!success) return NotFound(new { Status = "Error", Message = "Price not found" });
                return Ok(new { Status = "Success", Message = "Price deleted" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Status = "Error", Message = "Error deleting price", Error = ex.Message });
            }
        }

        // additional endpoints
        [HttpGet("ByVehicle/{vehicleId}")]
        public async Task<IActionResult> ByVehicle(int vehicleId)
        {
            try
            {
                var prices = await _priceService.GetPricesByVehicleIdAsync(vehicleId);
                return Ok(prices);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Status = "Error", Message = "Error retrieving prices", Error = ex.Message });
            }
        }

        [HttpGet("Active")]
        public async Task<IActionResult> Active()
        {
            try
            {
                var prices = await _priceService.GetActivePricesAsync();
                return Ok(prices);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Status = "Error", Message = "Error retrieving active prices", Error = ex.Message });
            }
        }
    }
}

using CarRentalApplication_API.Model;
using Microsoft.AspNetCore.Mvc;
using System.Data.Entity;
using CarRentalApplication_API.Services;  // where ILogService exists

namespace CarRentalApplication_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogService _logService;

        public UserController(ApplicationDbContext context, ILogService logService)
        {
            _context = context;
            _logService = logService;
        }

        // ========================= GET ALL ROLES =========================
        [HttpGet("GetAllRole")]
        public IActionResult GetRole()
        {
            try
            {
                var role = _context.Roles.ToList();

                _logService.AddLogAsync("Retrieved all roles successfully", "Info").Wait();

                return Ok(role);
            }
            catch (Exception ex)
            {
                _logService.AddLogAsync($"Error retrieving roles: {ex.Message}", "Error").Wait();

                return StatusCode(500, ex.Message);
            }
        }

        // ========================= GET ALL USERS =========================
        [HttpGet("GetAllUser")]
        public IActionResult GetUser()
        {
            try
            {
                var user = _context.Users.ToList();

                _logService.AddLogAsync("Retrieved all users successfully", "Info").Wait();

                return Ok(new
                {
                    Status = "Success",
                    Message = "User data retrieved successfully",
                    Data = user
                });
            }
            catch (Exception ex)
            {
                _logService.AddLogAsync($"Error retrieving users: {ex.Message}", "Error").Wait();

                return StatusCode(500, new
                {
                    Status = "Error",
                    Message = "An error occurred while retrieving user data",
                    Error = ex.Message
                });
            }
        }

        // ========================= ADD USER =========================
        [HttpPost("AddUser")]
        public IActionResult AddUser([FromBody] User user)
        {
            try
            {
                if (user == null)
                {
                    _logService.AddLogAsync("Attempted to add null user", "Warning").Wait();
                    return BadRequest("Invalid Data");
                }

                var existingUser = _context.Users
                    .FirstOrDefault(u => u.email == user.email || u.phone_Number == user.phone_Number);

                if (existingUser != null)
                {
                    _logService.AddLogAsync(
                        $"Duplicate user attempt (Email: {user.email}, Phone: {user.phone_Number})",
                        "Warning").Wait();

                    return Conflict(new
                    {
                        Status = "Error",
                        Message = "A user with the same email or phone number already exists"
                    });
                }

                user.createdAt = DateTime.UtcNow;

                _context.Users.Add(user);
                _context.SaveChanges();

                _logService.AddLogAsync($"User added successfully (ID: {user.user_id})", "Info").Wait();

                return Ok(new
                {
                    Status = "Success",
                    Message = "User added successfully",
                    Data = user
                });
            }
            catch (Exception ex)
            {
                _logService.AddLogAsync($"Error adding user: {ex.Message}", "Error").Wait();

                return StatusCode(500, new
                {
                    Status = "Error",
                    Message = "An error occurred while adding the user",
                    Error = ex.Message
                });
            }
        }

        // ========================= DELETE USER =========================
        [HttpDelete("DeleteUser/{id}")]
        public IActionResult DeleteUser(int id)
        {
            try
            {
                var user = _context.Users.Find(id);

                if (user == null)
                {
                    _logService.AddLogAsync($"User not found for deletion (ID: {id})", "Warning").Wait();

                    return NotFound(new
                    {
                        Status = "Error",
                        Message = "User not found"
                    });
                }

                _context.Users.Remove(user);
                _context.SaveChanges();

                _logService.AddLogAsync($"User deleted successfully (ID: {id})", "Warning").Wait();

                return Ok(new
                {
                    Status = "Success",
                    Message = "User deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logService.AddLogAsync($"Error deleting user (ID: {id}): {ex.Message}", "Error").Wait();

                return StatusCode(500, new
                {
                    Status = "Error",
                    Message = "An error occurred while deleting the user",
                    Error = ex.Message
                });
            }
        }

        // ========================= UPDATE USER =========================
        [HttpPut("UpdateUser/{id}")]
        public IActionResult UpdateUser(int id, [FromBody] User updatedUser)
        {
            try
            {
                var user = _context.Users.Find(id);

                if (user == null)
                {
                    _logService.AddLogAsync($"User not found for update (ID: {id})", "Warning").Wait();

                    return NotFound(new
                    {
                        Status = "Error",
                        Message = "User not found"
                    });
                }

                user.first_Name = updatedUser.first_Name;
                user.last_Name = updatedUser.last_Name;
                user.email = updatedUser.email;
                user.phone_Number = updatedUser.phone_Number;
                user.gender = updatedUser.gender;
                user.password = updatedUser.password;
                user.role_Id = updatedUser.role_Id;
                user.updatedAt = DateTime.UtcNow;

                _context.SaveChanges();

                _logService.AddLogAsync($"User updated successfully (ID: {id})", "Info").Wait();

                return Ok(new
                {
                    Status = "Success",
                    Message = "User updated successfully",
                    Data = user
                });
            }
            catch (Exception ex)
            {
                _logService.AddLogAsync($"Error updating user (ID: {id}): {ex.Message}", "Error").Wait();

                return StatusCode(500, new
                {
                    Status = "Error",
                    Message = "An error occurred while updating the user",
                    Error = ex.Message
                });
            }
        }

        // ========================= GET USER BY ID =========================
        [HttpGet("GetUserById/{id}")]
        public IActionResult GetUserById(int id)
        {
            try
            {
                var user = _context.Users.Find(id);

                if (user == null)
                {
                    _logService.AddLogAsync($"User not found (ID: {id})", "Warning").Wait();

                    return NotFound(new
                    {
                        Status = "Error",
                        Message = "User not found"
                    });
                }

                _logService.AddLogAsync($"User retrieved successfully (ID: {id})", "Info").Wait();

                return Ok(new
                {
                    Status = "Success",
                    Message = "User data retrieved successfully",
                    Data = user
                });
            }
            catch (Exception ex)
            {
                _logService.AddLogAsync($"Error retrieving user (ID: {id}): {ex.Message}", "Error").Wait();

                return StatusCode(500, new
                {
                    Status = "Error",
                    Message = "An error occurred while retrieving user data",
                    Error = ex.Message
                });
            }
        }
    }
}

using CarRentalApplication_API.Model;
using CarRentalApplication_API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRentalApplication_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class VehiclesController : Controller
    {
        private readonly IVehicleService _vehicleService;

        public VehiclesController(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }

        [HttpGet("GetAllVehicles")]
        public async Task<IActionResult> GetVehicles()
        {
            try
            {
                var vehicles = await _vehicleService.GetAllVehiclesAsync();
                return Ok(vehicles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Status = "Error",
                    Message = "An error occurred while retrieving vehicles",
                    Error = ex.Message
                });
            }
        }
        [HttpGet("GetVehicleById/{id}")]
        public async Task<IActionResult> GetVehicleById(int id)
        {
            try
            {
                var vehicle = await _vehicleService.GetVehicleByIdAsync(id);

                if (vehicle == null)
                    return NotFound(new { Status = "Error", Message = "Vehicle not found" });

                return Ok(vehicle);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Status = "Error",
                    Message = "Error retrieving vehicle",
                    Error = ex.Message
                });
            }
        }

        [HttpPost("AddVehicle")]
        public async Task<IActionResult> AddVehicle([FromBody] Vehicles vehicle)
        {
            try
            {
                var result = await _vehicleService.AddVehicleAsync(vehicle);

                return Ok(new
                {
                    Status = "Success",
                    Message = "Vehicle added successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Status = "Error",
                    Message = "Error adding vehicle",
                    Error = ex.Message
                });
            }
        }

        [HttpPut("UpdateVehicle/{id}")]
        public async Task<IActionResult> UpdateVehicle(int id, [FromBody] Vehicles updatedVehicle)
        {
            try
            {
                var result = await _vehicleService.UpdateVehicleAsync(id, updatedVehicle);

                if (result == null)
                    return NotFound(new { Status = "Error", Message = "Vehicle not found" });

                return Ok(new
                {
                    Status = "Success",
                    Message = "Vehicle updated successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Status = "Error",
                    Message = "Error updating vehicle",
                    Error = ex.Message
                });
            }
        }

        [HttpDelete("DeleteVehicle/{id}")]
        public async Task<IActionResult> DeleteVehicle(int id)
        {
            try
            {
                var deleted = await _vehicleService.DeleteVehicleAsync(id);

                if (!deleted)
                    return NotFound(new { Status = "Error", Message = "Vehicle not found" });

                return Ok(new
                {
                    Status = "Success",
                    Message = "Vehicle deleted successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Status = "Error",
                    Message = "Error deleting vehicle",
                    Error = ex.Message
                });
            }
        }

        [HttpPut("UpdateVehicleStatus/{id}")]
        public async Task<IActionResult> UpdateVehicleStatus(int id, [FromBody] string status)
        {
            try
            {
                var result = await _vehicleService.UpdateVehicleStatusAsync(id, status);

                if (result == null)
                    return NotFound(new { Status = "Error", Message = "Vehicle not found" });

                return Ok(new
                {
                    Status = "Success",
                    Message = "Vehicle status updated successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Status = "Error",
                    Message = "Error updating vehicle status",
                    Error = ex.Message
                });
            }
        }

        /* Get vehicle_Categories */
        [HttpGet("GetAllVehicle_Categories")]
        public async Task<IActionResult> GetAllVehicle_Categories()
        {
            try
            {
                var categories = await _vehicleService.GetAllVehicle_CategoriesAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Status = "Error",
                    Message = "Error retrieving vehicle categories",
                    Error = ex.Message
                });
            }
        }

        [HttpGet("GetVehicle_CategoryById/{id}")]
        public async Task<IActionResult> GetVehicle_CategoryById(int id)
        {
            try
            {
                var category = await _vehicleService.GetVehicle_CategoryByIdAsync(id);
                if (category == null)
                    return NotFound(new { Status = "Error", Message = "Vehicle category not found" });
                return Ok(category);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Status = "Error",
                    Message = "Error retrieving vehicle category",
                    Error = ex.Message
                });
            }
        }
        [HttpPost("AddVehicle_Category")]
        public async Task<IActionResult> AddVehicle_Category([FromBody] vehicle_categories category)
        {
            try
            {
                var result = await _vehicleService.AddVehicle_CategoryAsync(category);
                return Ok(new
                {
                    Status = "Success",
                    Message = "Vehicle category added successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Status = "Error",
                    Message = "Error adding vehicle category",
                    Error = ex.Message
                });
            }


        }

        [HttpPut("UpdateVehicle_Category/{id}")]
        public async Task<IActionResult> UpdateVehicle_Category(int id, [FromBody] vehicle_categories updatedCategory)
        {
            try
            {
                var result = await _vehicleService.UpdateVehicle_CategoryAsync(id, updatedCategory);
                if (result == null)
                    return NotFound(new { Status = "Error", Message = "Vehicle category not found" });
                return Ok(new
                {
                    Status = "Success",
                    Message = "Vehicle category updated successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Status = "Error",
                    Message = "Error updating vehicle category",
                    Error = ex.Message
                });
            }
        }

        [HttpDelete("DeleteVehicle_Category/{id}")]
        public async Task<IActionResult> DeleteVehicle_Category(int id)
        {
            try
            {
                var deleted = await _vehicleService.DeleteVehicle_CategoryAsync(id);
                if (!deleted)
                    return NotFound(new { Status = "Error", Message = "Vehicle category not found" });
                return Ok(new
                {
                    Status = "Success",
                    Message = "Vehicle category deleted successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Status = "Error",
                    Message = "Error deleting vehicle category",
                    Error = ex.Message
                });
            }
        }
    }
}

using Microsoft.AspNetCore.Mvc;

namespace CarRentalApplication_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StudentMangementSystem.Data;
using StudentMangementSystem.Models;

namespace StudentMangementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CityController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CityController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("GetAllCity")]
        public IActionResult Get()
        {
            try
            {
                var city = _context.CityMasters.ToList();
                return Ok(city);
            }
            catch (Exception ex)
            {

                return BadRequest(new
                {
                    Message = "An error occurred while fetching city data.",
                    Error = ex.Message
                });
            }
        }

        [HttpPost("CreateNewCity")]
        public IActionResult Add(CityMaster city)
        {
            try
            {
                var cityExist = _context.CityMasters.FirstOrDefault(c => c.CityName == city.CityName);
                if (cityExist == null)
                {
                    _context.CityMasters.Add(city);
                    _context.SaveChanges();
                    return Created("City Created", cityExist);
                }
                else
                {
                    return BadRequest("City Alrady Exist");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Message = "City Already Exist",
                    Error = ex.Message
                });

            }
        }
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StudentMangementSystem.Data;
using StudentMangementSystem.Models;

namespace StudentMangementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EmployeesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EmployeesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("GetAllEmployeeAndBankDeatils")]
        public IActionResult GetAllEmployeeAndBankDeatils()
        {
            try
            {
                var list = (from e in _context.Employees
                            join
                              b in _context.BankDeatails
                              on e.employee_id equals b.employee_id
                            select new
                            {
                                e.first_name,
                                e.last_name,
                                e.hire_date,
                                b.BankName,
                                b.AccountNumber,
                                b.IFSCCode
                            }).ToList();

                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpPost("CreateEmployee")]
        public IActionResult CreateNewEmployee(EmpBankViewModel obj)
        {
            try
            {
                Employees emp = new Employees()
                {
                    first_name = obj.first_name,
                    last_name = obj.last_name,
                    hire_date = obj.hire_date
                };
                _context.Employees.Add(emp);
                _context.SaveChanges();

                BankDeatails bankDetails = new BankDeatails()
                {
                    employee_id = emp.employee_id,
                    AccountNumber = obj.AccountNumber,
                    BankName = obj.BankName,
                    IFSCCode = obj.IFSCCode,

                };
                _context.BankDeatails.Add(bankDetails);
                _context.SaveChanges();

                return Ok("Employee and Bank Details created successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }

        }
    }
}

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StudentMangementSystem.Data;
using StudentMangementSystem.Models;
using StudentMangementSystem.ViewModels;

namespace StudentMangementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtSettings _jwtSettings;

        public LoginController(ApplicationDbContext context, IOptions<JwtSettings> jwtOptions)
        {
            _context = context;
            _jwtSettings = jwtOptions.Value;
        }

        [HttpPost("BatchLogin")]
        public async Task<IActionResult> Login([FromBody] LoginViewModel request)
        {
            try
            {
                if (request == null || string.IsNullOrEmpty(request.UserName) || string.IsNullOrEmpty(request.Password))
                {
                    return BadRequest("Username and password are required.");
                }

                var student = await _context.Students
                    .FirstOrDefaultAsync(s => s.UserName == request.UserName && s.Password == request.Password);

                if (student == null)
                {
                    return Unauthorized("Invalid username or password.");
                }

                if (!student.IsActive)
                {
                    return Unauthorized("This account is not active. Contact support.");
                }

                // create claims
                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, student.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim("studentId", student.Id.ToString()),
                    new Claim("fullName", $"{student.FName} {student.LName}")
                };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var expires = DateTime.UtcNow.AddMinutes(_jwtSettings.DurationMinutes > 0 ? _jwtSettings.DurationMinutes : 60);

                var token = new JwtSecurityToken(
                    issuer: _jwtSettings.Issuer,
                    audience: _jwtSettings.Audience,
                    claims: claims,
                    expires: expires,
                    signingCredentials: creds
                );

                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                return Ok(new
                {
                    message = "Login successful",
                    token = tokenString,
                    expires = expires,
                    studentId = student.Id,
                    userName = student.UserName,
                    fullName = $"{student.FName} {student.LName}",
                    email = student.Email
                });
            }
            catch (Exception ex)
            {
                // log ex if needed
                return StatusCode(500, "An internal error occurred while processing the login.");
            }
        }

    }
}

using Microsoft.AspNetCore.Mvc;
using StudentMangementSystem.Data;

namespace StudentMangementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MasterController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MasterController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("GetAllCountries")]
        public IActionResult GetAllCountries()
        {
            try
            {
                var list = _context.CountryMasters.ToList();
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("getStateByCountryId")]
        public IActionResult GetStateByCountry(int CountryId)
        {
            try
            {
                var statelist = _context.StateMasters.Where(s => s.countryId == CountryId).ToList();
                return Ok(statelist);

            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("getDistrictByStateId")]
        public IActionResult GetDistrictByStateId(int StateId)
        {
            try
            {
                var districtlist = _context.DistrictMasters.Where(s => s.stateId == StateId).ToList();
                return Ok(districtlist);

            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpDelete(("DeleteDistrict/{id}"))]
        public IActionResult Delete(int id)
        {
            try
            {

                var delete = _context.DistrictMasters.Where(s => s.districtId == id).FirstOrDefault();
                if (delete == null)
                {
                    return NotFound();
                }
                _context.DistrictMasters.Remove(delete);
                _context.SaveChanges();
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}


using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentMangementSystem.Data;
using StudentMangementSystem.Models;

namespace StudentMangementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StudentController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetStudents()
        {
            var students = await _context.Students.ToListAsync();
            return Ok(students);
        }

        [HttpPost]
        public async Task<IActionResult> CreateStudent(Student student)
        {
            try
            {
                if (student == null)
                {
                    return BadRequest("Enter all Values");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                _context.Students.Add(student);
                await _context.SaveChangesAsync();
                return Ok(student);
            }

            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while saving the student record.");
            }

        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            try
            {
                var student = await _context.Students.FindAsync(id);

                if (student == null)
                {
                    return NotFound($"Student with Id {id} not found.");
                }
                _context.Students.Remove(student);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while deleting the student.");
            }

        }
    }
}

