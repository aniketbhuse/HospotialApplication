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




/* Demo Code*/

using auto.ecom.api.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace auto.ecom.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartMasterController : Controller
    {
        private readonly AutoDbContext _context;

        public CartMasterController(AutoDbContext context)
        {
            _context = context;
        }
        [HttpGet("GetALL")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await (
                                    from cmt in _context.cartMasterTbls
                                    join pm in _context.productMasters on cmt.productId equals pm.productId
                                    join cm in _context.Customers on cmt.customerId equals cm.custId
                                    select new
                                    {
                                        cmt.quantity,
                                        cmt.addedDate,
                                        cmt.modifiedDate,
                                        pm.shortName,
                                        pm.fullName,
                                        pm.price,
                                        pm.description,
                                        pm.thumbnailImage,
                                        pm.productImage,
                                        pm.sku,
                                        cm.customerName,
                                        cm.mobileNo,
                                        cm.email,
                                        cm.city,
                                        cm.address,
                                        cm.pincode
                                    }).ToListAsync();

                //var data = await _context.Set<cartMasterTbl>().ToListAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // GET CART ITEM BY ID
        [HttpGet("GetCartItemById{id}")]
        public async Task<IActionResult> GetCartItemById(int id)
        {
            try
            {
                var cart = await (
                                    from cmt in _context.cartMasterTbls
                                    join pm in _context.productMasters on cmt.productId equals pm.productId
                                    //join cm in _context.Customers on cmt.customerId equals cm.custId
                                    where cmt.cartId == id
                                    select new
                                    {
                                        cmt.quantity,
                                        cmt.addedDate,
                                        cmt.modifiedDate,
                                        pm.shortName,
                                        pm.fullName,
                                        pm.price,
                                        pm.description,
                                        pm.thumbnailImage,
                                        pm.productImage,
                                        pm.sku,
                                        //cm.customerName,
                                        //cm.mobileNo,
                                        //cm.email,
                                        // cm.city,
                                        //cm.address,
                                        // cm.pincode
                                    }).ToListAsync();

                //var cart = await _context.Set<cartMasterTbl>().FindAsync(id);
                if (cart == null)
                    return NotFound("Cart item not found");

                return Ok(cart);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // ADD TO CART
        [HttpPost("AddToCart")]
        public async Task<IActionResult> Create(cartMasterTbl model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                model.addedDate = DateTime.Now;
                model.modifiedDate = DateTime.Now;

                await _context.Set<cartMasterTbl>().AddAsync(model);
                await _context.SaveChangesAsync();

                return Ok("Item added to cart successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // UPDATE CART ITEM
        [HttpPut("UpdateCartItemById/{id}")]
        public async Task<IActionResult> Update(int id, cartMasterTbl model)
        {
            try
            {
                var existing = await _context.Set<cartMasterTbl>().FindAsync(id);
                if (existing == null)
                    return NotFound("Cart item not found");

                model.modifiedDate = DateTime.Now;
                _context.Entry(existing).CurrentValues.SetValues(model);
                await _context.SaveChangesAsync();

                return Ok("Cart item updated successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // DELETE CART ITEM
        [HttpDelete("DeleteCartItemById/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var cart = await _context.Set<cartMasterTbl>().FindAsync(id);
                if (cart == null)
                    return NotFound("Cart item not found");

                _context.Set<cartMasterTbl>().Remove(cart);
                await _context.SaveChangesAsync();

                return Ok("Cart item removed successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
    }
}

using auto.ecom.api.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace auto.ecom.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly AutoDbContext _context;

        public CategoryController(AutoDbContext context)
        {
            _context = context;
        }

        [HttpGet("getAllCategory")]
        public IActionResult GetCategories()
        {
            var categories = _context.CategoryMasters.ToList();
            return Ok(categories);
        }

        [HttpPost("SaveCategory")]
        public IActionResult SaveCategory([FromBody] CategoryMaster obj)
        {
            _context.CategoryMasters.Add(obj);
            _context.SaveChanges();
            return Ok(obj);
        }

        [HttpPut("UpdateCategory")]
        public IActionResult UpdateCategory([FromBody] CategoryMaster obj)
        {
            var existingCategory = _context.CategoryMasters.Find(obj.categoryId);
            if (existingCategory == null)
            {
                return NotFound("Category not found");
            }
            else
            {
                existingCategory.imageName = obj.imageName;
                existingCategory.categoryName = obj.categoryName;
                return Ok(obj);
            }
        }


        [HttpGet("DeleteCategoryById")]
        public IActionResult DeleteCategoryById(int id)
        {
            var existingCategory = _context.CategoryMasters.Find(id);
            if (existingCategory == null)
            {
                return NotFound("Category not found");
            }
            else
            {
                _context.CategoryMasters.Remove(existingCategory);
                _context.SaveChanges();
                return Ok("Category Deleted Success");
            }
        }


        [HttpPost("add")]
        public async Task<IActionResult> AddChildCategory([FromBody] ChildCategoryMaster model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Check duplicate name globally
                bool nameExists = await _context.ChildCategoryMasters
                    .AnyAsync(x => x.childCategoryName.Trim().ToLower() == model.childCategoryName.Trim().ToLower());

                if (nameExists)
                    return BadRequest("Child category name already exists.");

                // Check duplicate in same category
                bool categoryNameExists = await _context.ChildCategoryMasters
                    .AnyAsync(x => x.categoryId == model.categoryId &&
                                   x.childCategoryName.Trim().ToLower() == model.childCategoryName.Trim().ToLower());

                if (categoryNameExists)
                    return BadRequest("Child category already exists for this category.");

                _context.ChildCategoryMasters.Add(model);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Child category added successfully.",
                    data = model
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // ============================
        // GET ALL
        // ============================
        [HttpGet("list-with-category")]
        public async Task<IActionResult> GetAllWithCategory()
        {
            try
            {
                var data = await (from cc in _context.ChildCategoryMasters
                                  join c in _context.CategoryMasters on cc.categoryId equals c.categoryId
                                  select new ChildCategoryWithCategoryDto
                                  {
                                      childCategoryId = cc.childCategoryId,
                                      childCategoryName = cc.childCategoryName,
                                      categoryId = cc.categoryId,
                                      categoryName = c.categoryName,
                                      imageName = c.imageName
                                  }).ToListAsync();

                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // ============================
        // GET BY ID
        // ============================
        [HttpGet("get/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var data = await _context.ChildCategoryMasters.FindAsync(id);

                if (data == null)
                    return NotFound("Child category not found.");

                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // ============================
        // UPDATE
        // ============================
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ChildCategoryMaster model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var existing = await _context.ChildCategoryMasters.FindAsync(id);
                if (existing == null)
                    return NotFound("Child category not found.");

                // Duplicate name check (ignore current id)
                bool nameExists = await _context.ChildCategoryMasters
                    .AnyAsync(x => x.childCategoryName.Trim().ToLower() == model.childCategoryName.Trim().ToLower() &&
                                   x.childCategoryId != id);

                if (nameExists)
                    return BadRequest("Child category name already exists.");

                // Duplicate in same category
                bool categoryNameExists = await _context.ChildCategoryMasters
                    .AnyAsync(x => x.categoryId == model.categoryId &&
                                   x.childCategoryName.Trim().ToLower() == model.childCategoryName.Trim().ToLower() &&
                                   x.childCategoryId != id);

                if (categoryNameExists)
                    return BadRequest("Child category already exists for this category.");

                existing.childCategoryName = model.childCategoryName;
                existing.categoryId = model.categoryId;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Child category updated successfully.",
                    data = existing
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // ============================
        // DELETE
        // ============================
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var existing = await _context.ChildCategoryMasters.FindAsync(id);
                if (existing == null)
                    return NotFound("Child category not found.");

                _context.ChildCategoryMasters.Remove(existing);
                await _context.SaveChangesAsync();

                return Ok("Child category deleted successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}

///


using auto.ecom.api.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace auto.ecom.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderDetailController : Controller
    {
        private readonly AutoDbContext _context;

        public OrderDetailController(AutoDbContext context)
        {
            _context = context;
        }

        // GET ALL ORDER DETAILS
        [HttpGet("GetAllOrders")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var list = await (
                    from ot in _context.orderDetailTabls
                    join pm in _context.productMasters on ot.productId equals pm.productId
                    join om in _context.orderMasters on ot.orderId equals om.orderId
                    select new
                    {
                        ot.quantity,
                        pm.shortName,
                        pm.fullName,
                        pm.price,
                        pm.description,
                        pm.thumbnailImage,
                        pm.productImage,
                        pm.sku,
                        om.orderDate,
                        om.totalAmout,
                        om.paymentNaration
                    }).ToListAsync();
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // GET BY ORDER DETAIL ID
        [HttpGet("GetOrderDetailsById{id}")]
        public async Task<IActionResult> GetOrderDetailsById(int id)
        {
            try
            {
                var details = await (
                    from ot in _context.orderDetailTabls
                    join pm in _context.productMasters on ot.productId equals pm.productId
                    join om in _context.orderMasters on ot.orderId equals om.orderId
                    where ot.orderDetailId == id
                    select new
                    {
                        ot.quantity,
                        pm.shortName,
                        pm.fullName,
                        pm.price,
                        pm.description,
                        pm.thumbnailImage,
                        pm.productImage,
                        pm.sku,
                        om.orderDate,
                        om.totalAmout,
                        om.paymentNaration
                    }).ToListAsync();

                if (details == null || details.Count == 0)
                    return NotFound("No order details found for this order.");

                return Ok(details);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // CREATE ORDER DETAIL (Add Product to Order)
        [HttpPost("AddItem")]
        public async Task<IActionResult> AddOrderItem(orderDetailTabl model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Prevent duplicate product entry inside the same order
                var exists = await _context.Set<orderDetailTabl>()
                    .AnyAsync(x => x.orderId == model.orderId && x.productId == model.productId);

                if (exists)
                    return BadRequest("Product already added to this order. Update quantity instead!");

                await _context.Set<orderDetailTabl>().AddAsync(model);
                await _context.SaveChangesAsync();

                return Ok("Product added to order successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // UPDATE ITEM QUANTITY
        [HttpPut("UpdateQuantity/{id}")]
        public async Task<IActionResult> UpdateQuantity(int id, int quantity)
        {
            try
            {
                var orderDetail = await _context.Set<orderDetailTabl>().FindAsync(id);
                if (orderDetail == null)
                    return NotFound("Order detail not found.");

                orderDetail.quantity = quantity;
                await _context.SaveChangesAsync();

                return Ok("Quantity updated successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // DELETE AN ORDER ITEM
        [HttpDelete("DeleteOrderItemById{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var detail = await _context.Set<orderDetailTabl>().FindAsync(id);
                if (detail == null)
                    return NotFound("Order detail not found.");

                _context.Set<orderDetailTabl>().Remove(detail);
                await _context.SaveChangesAsync();

                return Ok("Order item removed.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
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
    public class OrderDetailController : Controller
    {
        private readonly AutoDbContext _context;

        public OrderDetailController(AutoDbContext context)
        {
            _context = context;
        }

        // GET ALL ORDER DETAILS
        [HttpGet("GetAllOrders")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var list = await (
                    from ot in _context.orderDetailTabls
                    join pm in _context.productMasters on ot.productId equals pm.productId
                    join om in _context.orderMasters on ot.orderId equals om.orderId
                    select new
                    {
                        ot.quantity,
                        pm.shortName,
                        pm.fullName,
                        pm.price,
                        pm.description,
                        pm.thumbnailImage,
                        pm.productImage,
                        pm.sku,
                        om.orderDate,
                        om.totalAmout,
                        om.paymentNaration
                    }).ToListAsync();
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // GET BY ORDER DETAIL ID
        [HttpGet("GetOrderDetailsById{id}")]
        public async Task<IActionResult> GetOrderDetailsById(int id)
        {
            try
            {
                var details = await (
                    from ot in _context.orderDetailTabls
                    join pm in _context.productMasters on ot.productId equals pm.productId
                    join om in _context.orderMasters on ot.orderId equals om.orderId
                    where ot.orderDetailId == id
                    select new
                    {
                        ot.quantity,
                        pm.shortName,
                        pm.fullName,
                        pm.price,
                        pm.description,
                        pm.thumbnailImage,
                        pm.productImage,
                        pm.sku,
                        om.orderDate,
                        om.totalAmout,
                        om.paymentNaration
                    }).ToListAsync();

                if (details == null || details.Count == 0)
                    return NotFound("No order details found for this order.");

                return Ok(details);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // CREATE ORDER DETAIL (Add Product to Order)
        [HttpPost("AddItem")]
        public async Task<IActionResult> AddOrderItem(orderDetailTabl model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Prevent duplicate product entry inside the same order
                var exists = await _context.Set<orderDetailTabl>()
                    .AnyAsync(x => x.orderId == model.orderId && x.productId == model.productId);

                if (exists)
                    return BadRequest("Product already added to this order. Update quantity instead!");

                await _context.Set<orderDetailTabl>().AddAsync(model);
                await _context.SaveChangesAsync();

                return Ok("Product added to order successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // UPDATE ITEM QUANTITY
        [HttpPut("UpdateQuantity/{id}")]
        public async Task<IActionResult> UpdateQuantity(int id, int quantity)
        {
            try
            {
                var orderDetail = await _context.Set<orderDetailTabl>().FindAsync(id);
                if (orderDetail == null)
                    return NotFound("Order detail not found.");

                orderDetail.quantity = quantity;
                await _context.SaveChangesAsync();

                return Ok("Quantity updated successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // DELETE AN ORDER ITEM
        [HttpDelete("DeleteOrderItemById{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var detail = await _context.Set<orderDetailTabl>().FindAsync(id);
                if (detail == null)
                    return NotFound("Order detail not found.");

                _context.Set<orderDetailTabl>().Remove(detail);
                await _context.SaveChangesAsync();

                return Ok("Order item removed.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
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
    public class productMasterController : Controller
    {
        private readonly AutoDbContext _context;

        public productMasterController(AutoDbContext context)
        {
            _context = context;
        }

        //  GET ALL PRODUCT 
        [HttpGet("GetAllProduct")]
        public IActionResult GetAllProduct()
        {
            try
            {
                var customers = (from pm in _context.productMasters
                                 join ccm in _context.ChildCategoryMasters
                                 on pm.childCategoryId equals ccm.childCategoryId
                                 select new
                                 {
                                     pm.shortName,
                                     pm.fullName,
                                     pm.price,
                                     pm.description,
                                     pm.thumbnailImage,
                                     pm.productImage,
                                     pm.sku,
                                     ccm.childCategoryName
                                 }).ToList();
                //var customers = _context.productMasters.ToList();
                return Ok(customers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // GET BY ID
        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var product1 = await (from pm in _context.productMasters
                                      join cm in _context.ChildCategoryMasters
                                      on pm.childCategoryId equals cm.childCategoryId
                                      where pm.productId == id
                                      select new
                                      {
                                          pm.shortName,
                                          pm.fullName,
                                          pm.price,
                                          pm.description,
                                          pm.thumbnailImage,
                                          pm.productImage,
                                          pm.sku,
                                          cm.childCategoryName
                                      }).FirstOrDefaultAsync();
                // var product = await _context.Set<productMaster>().FindAsync(id);
                if (product1 == null)
                    return NotFound("Product not found");

                return Ok(product1);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // CREATE
        [HttpPost("AddProduct")]
        public async Task<IActionResult> AddProduct(productMaster model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                //  Check for duplicate by SKU (or you can use shortName/fullName)
                var exists = await _context.Set<productMaster>()
                                           .AnyAsync(p => p.sku == model.sku);

                if (exists)
                    return BadRequest("Product with the same SKU already exists!"); // Duplicate message

                // If not duplicate → save product
                await _context.Set<productMaster>().AddAsync(model);
                await _context.SaveChangesAsync();

                return Ok("Product created successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpPut("UpdateProduct/{id}")]
        public async Task<IActionResult> UpdateProduct(int id, productMaster model)
        {
            try
            {
                var existing = await _context.Set<productMaster>().FindAsync(id);
                if (existing == null)
                    return NotFound("Product not found");

                _context.Entry(existing).CurrentValues.SetValues(model);
                await _context.SaveChangesAsync();

                return Ok("Product updated successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // DELETE
        [HttpDelete("DeleteProduct/{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var product = await _context.Set<productMaster>().FindAsync(id);
                if (product == null)
                    return NotFound("Product not found");

                _context.Set<productMaster>().Remove(product);
                await _context.SaveChangesAsync();

                return Ok("Product deleted successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
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



using auto.ecom.api.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace auto.ecom.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderDetailController : Controller
    {
        private readonly AutoDbContext _context;

        public OrderDetailController(AutoDbContext context)
        {
            _context = context;
        }

        // GET ALL ORDER DETAILS
        [HttpGet("GetAllOrders")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var list = await (
                    from ot in _context.orderDetailTabls
                    join pm in _context.productMasters on ot.productId equals pm.productId
                    join om in _context.orderMasters on ot.orderId equals om.orderId
                    select new
                    {
                        ot.quantity,
                        pm.shortName,
                        pm.fullName,
                        pm.price,
                        pm.description,
                        pm.thumbnailImage,
                        pm.productImage,
                        pm.sku,
                        om.orderDate,
                        om.totalAmout,
                        om.paymentNaration
                    }).ToListAsync();
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // GET BY ORDER DETAIL ID
        [HttpGet("GetOrderDetailsById{id}")]
        public async Task<IActionResult> GetOrderDetailsById(int id)
        {
            try
            {
                var details = await (
                    from ot in _context.orderDetailTabls
                    join pm in _context.productMasters on ot.productId equals pm.productId
                    join om in _context.orderMasters on ot.orderId equals om.orderId
                    where ot.orderDetailId == id
                    select new
                    {
                        ot.quantity,
                        pm.shortName,
                        pm.fullName,
                        pm.price,
                        pm.description,
                        pm.thumbnailImage,
                        pm.productImage,
                        pm.sku,
                        om.orderDate,
                        om.totalAmout,
                        om.paymentNaration
                    }).ToListAsync();

                if (details == null || details.Count == 0)
                    return NotFound("No order details found for this order.");

                return Ok(details);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // CREATE ORDER DETAIL (Add Product to Order)
        [HttpPost("AddItem")]
        public async Task<IActionResult> AddOrderItem(orderDetailTabl model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Prevent duplicate product entry inside the same order
                var exists = await _context.Set<orderDetailTabl>()
                    .AnyAsync(x => x.orderId == model.orderId && x.productId == model.productId);

                if (exists)
                    return BadRequest("Product already added to this order. Update quantity instead!");

                await _context.Set<orderDetailTabl>().AddAsync(model);
                await _context.SaveChangesAsync();

                return Ok("Product added to order successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // UPDATE ITEM QUANTITY
        [HttpPut("UpdateQuantity/{id}")]
        public async Task<IActionResult> UpdateQuantity(int id, int quantity)
        {
            try
            {
                var orderDetail = await _context.Set<orderDetailTabl>().FindAsync(id);
                if (orderDetail == null)
                    return NotFound("Order detail not found.");

                orderDetail.quantity = quantity;
                await _context.SaveChangesAsync();

                return Ok("Quantity updated successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // DELETE AN ORDER ITEM
        [HttpDelete("DeleteOrderItemById{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var detail = await _context.Set<orderDetailTabl>().FindAsync(id);
                if (detail == null)
                    return NotFound("Order detail not found.");

                _context.Set<orderDetailTabl>().Remove(detail);
                await _context.SaveChangesAsync();

                return Ok("Order item removed.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
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
    public class OrderDetailController : Controller
    {
        private readonly AutoDbContext _context;

        public OrderDetailController(AutoDbContext context)
        {
            _context = context;
        }

        // GET ALL ORDER DETAILS
        [HttpGet("GetAllOrders")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var list = await (
                    from ot in _context.orderDetailTabls
                    join pm in _context.productMasters on ot.productId equals pm.productId
                    join om in _context.orderMasters on ot.orderId equals om.orderId
                    select new
                    {
                        ot.quantity,
                        pm.shortName,
                        pm.fullName,
                        pm.price,
                        pm.description,
                        pm.thumbnailImage,
                        pm.productImage,
                        pm.sku,
                        om.orderDate,
                        om.totalAmout,
                        om.paymentNaration
                    }).ToListAsync();
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // GET BY ORDER DETAIL ID
        [HttpGet("GetOrderDetailsById{id}")]
        public async Task<IActionResult> GetOrderDetailsById(int id)
        {
            try
            {
                var details = await (
                    from ot in _context.orderDetailTabls
                    join pm in _context.productMasters on ot.productId equals pm.productId
                    join om in _context.orderMasters on ot.orderId equals om.orderId
                    where ot.orderDetailId == id
                    select new
                    {
                        ot.quantity,
                        pm.shortName,
                        pm.fullName,
                        pm.price,
                        pm.description,
                        pm.thumbnailImage,
                        pm.productImage,
                        pm.sku,
                        om.orderDate,
                        om.totalAmout,
                        om.paymentNaration
                    }).ToListAsync();

                if (details == null || details.Count == 0)
                    return NotFound("No order details found for this order.");

                return Ok(details);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // CREATE ORDER DETAIL (Add Product to Order)
        [HttpPost("AddItem")]
        public async Task<IActionResult> AddOrderItem(orderDetailTabl model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Prevent duplicate product entry inside the same order
                var exists = await _context.Set<orderDetailTabl>()
                    .AnyAsync(x => x.orderId == model.orderId && x.productId == model.productId);

                if (exists)
                    return BadRequest("Product already added to this order. Update quantity instead!");

                await _context.Set<orderDetailTabl>().AddAsync(model);
                await _context.SaveChangesAsync();

                return Ok("Product added to order successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // UPDATE ITEM QUANTITY
        [HttpPut("UpdateQuantity/{id}")]
        public async Task<IActionResult> UpdateQuantity(int id, int quantity)
        {
            try
            {
                var orderDetail = await _context.Set<orderDetailTabl>().FindAsync(id);
                if (orderDetail == null)
                    return NotFound("Order detail not found.");

                orderDetail.quantity = quantity;
                await _context.SaveChangesAsync();

                return Ok("Quantity updated successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // DELETE AN ORDER ITEM
        [HttpDelete("DeleteOrderItemById{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var detail = await _context.Set<orderDetailTabl>().FindAsync(id);
                if (detail == null)
                    return NotFound("Order detail not found.");

                _context.Set<orderDetailTabl>().Remove(detail);
                await _context.SaveChangesAsync();

                return Ok("Order item removed.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
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
    public class productMasterController : Controller
    {
        private readonly AutoDbContext _context;

        public productMasterController(AutoDbContext context)
        {
            _context = context;
        }

        //  GET ALL PRODUCT 
        [HttpGet("GetAllProduct")]
        public IActionResult GetAllProduct()
        {
            try
            {
                var customers = (from pm in _context.productMasters
                                 join ccm in _context.ChildCategoryMasters
                                 on pm.childCategoryId equals ccm.childCategoryId
                                 select new
                                 {
                                     pm.shortName,
                                     pm.fullName,
                                     pm.price,
                                     pm.description,
                                     pm.thumbnailImage,
                                     pm.productImage,
                                     pm.sku,
                                     ccm.childCategoryName
                                 }).ToList();
                //var customers = _context.productMasters.ToList();
                return Ok(customers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // GET BY ID
        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var product1 = await (from pm in _context.productMasters
                                      join cm in _context.ChildCategoryMasters
                                      on pm.childCategoryId equals cm.childCategoryId
                                      where pm.productId == id
                                      select new
                                      {
                                          pm.shortName,
                                          pm.fullName,
                                          pm.price,
                                          pm.description,
                                          pm.thumbnailImage,
                                          pm.productImage,
                                          pm.sku,
                                          cm.childCategoryName
                                      }).FirstOrDefaultAsync();
                // var product = await _context.Set<productMaster>().FindAsync(id);
                if (product1 == null)
                    return NotFound("Product not found");

                return Ok(product1);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // CREATE
        [HttpPost("AddProduct")]
        public async Task<IActionResult> AddProduct(productMaster model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                //  Check for duplicate by SKU (or you can use shortName/fullName)
                var exists = await _context.Set<productMaster>()
                                           .AnyAsync(p => p.sku == model.sku);

                if (exists)
                    return BadRequest("Product with the same SKU already exists!"); // Duplicate message

                // If not duplicate → save product
                await _context.Set<productMaster>().AddAsync(model);
                await _context.SaveChangesAsync();

                return Ok("Product created successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpPut("UpdateProduct/{id}")]
        public async Task<IActionResult> UpdateProduct(int id, productMaster model)
        {
            try
            {
                var existing = await _context.Set<productMaster>().FindAsync(id);
                if (existing == null)
                    return NotFound("Product not found");

                _context.Entry(existing).CurrentValues.SetValues(model);
                await _context.SaveChangesAsync();

                return Ok("Product updated successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // DELETE
        [HttpDelete("DeleteProduct/{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var product = await _context.Set<productMaster>().FindAsync(id);
                if (product == null)
                    return NotFound("Product not found");

                _context.Set<productMaster>().Remove(product);
                await _context.SaveChangesAsync();

                return Ok("Product deleted successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
    }
}



///



using Microsoft.AspNetCore.Mvc;
using HealthcareApp.Models;
using HealthcareApp.Data;

namespace HealthcareApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly ApplicationDbContext _context;

        public AccountController(ILogger<AccountController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // GET
        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            var model = new LoginViewModel { ReturnUrl = returnUrl };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = _context.Users.FirstOrDefault(u =>
                u.Email.Trim().ToLower() == model.Username.Trim().ToLower()
                && u.Password == model.Password);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid username or password");
                return View(model);
            }

            // Session set
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserName", user.FullName);
            HttpContext.Session.SetString("UserRoleId", user.RoleId.ToString());

            Console.WriteLine("Login Success: " + user.Email);

            // ✅ Redirect
            if (user.RoleId == 2)
                return RedirectToAction("Index", "Doctor");

            else if (user.RoleId == 1)
                return RedirectToAction("Index", "Admin");

            else
                return RedirectToAction("Index", "Dashboard");
        }

        // Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // Protected page example
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("UserEmail") == null)
            {
                return RedirectToAction("Login");
            }

            return View();
        }
    }
}


/* Demo Code*/

using auto.ecom.api.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace auto.ecom.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartMasterController : Controller
    {
        private readonly AutoDbContext _context;

        public CartMasterController(AutoDbContext context)
        {
            _context = context;
        }
        [HttpGet("GetALL")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await (
                                    from cmt in _context.cartMasterTbls
                                    join pm in _context.productMasters on cmt.productId equals pm.productId
                                    join cm in _context.Customers on cmt.customerId equals cm.custId
                                    select new
                                    {
                                        cmt.quantity,
                                        cmt.addedDate,
                                        cmt.modifiedDate,
                                        pm.shortName,
                                        pm.fullName,
                                        pm.price,
                                        pm.description,
                                        pm.thumbnailImage,
                                        pm.productImage,
                                        pm.sku,
                                        cm.customerName,
                                        cm.mobileNo,
                                        cm.email,
                                        cm.city,
                                        cm.address,
                                        cm.pincode
                                    }).ToListAsync();

                //var data = await _context.Set<cartMasterTbl>().ToListAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // GET CART ITEM BY ID
        [HttpGet("GetCartItemById{id}")]
        public async Task<IActionResult> GetCartItemById(int id)
        {
            try
            {
                var cart = await (
                                    from cmt in _context.cartMasterTbls
                                    join pm in _context.productMasters on cmt.productId equals pm.productId
                                    //join cm in _context.Customers on cmt.customerId equals cm.custId
                                    where cmt.cartId == id
                                    select new
                                    {
                                        cmt.quantity,
                                        cmt.addedDate,
                                        cmt.modifiedDate,
                                        pm.shortName,
                                        pm.fullName,
                                        pm.price,
                                        pm.description,
                                        pm.thumbnailImage,
                                        pm.productImage,
                                        pm.sku,
                                        //cm.customerName,
                                        //cm.mobileNo,
                                        //cm.email,
                                        // cm.city,
                                        //cm.address,
                                        // cm.pincode
                                    }).ToListAsync();

                //var cart = await _context.Set<cartMasterTbl>().FindAsync(id);
                if (cart == null)
                    return NotFound("Cart item not found");

                return Ok(cart);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // ADD TO CART
        [HttpPost("AddToCart")]
        public async Task<IActionResult> Create(cartMasterTbl model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                model.addedDate = DateTime.Now;
                model.modifiedDate = DateTime.Now;

                await _context.Set<cartMasterTbl>().AddAsync(model);
                await _context.SaveChangesAsync();

                return Ok("Item added to cart successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // UPDATE CART ITEM
        [HttpPut("UpdateCartItemById/{id}")]
        public async Task<IActionResult> Update(int id, cartMasterTbl model)
        {
            try
            {
                var existing = await _context.Set<cartMasterTbl>().FindAsync(id);
                if (existing == null)
                    return NotFound("Cart item not found");

                model.modifiedDate = DateTime.Now;
                _context.Entry(existing).CurrentValues.SetValues(model);
                await _context.SaveChangesAsync();

                return Ok("Cart item updated successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // DELETE CART ITEM
        [HttpDelete("DeleteCartItemById/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var cart = await _context.Set<cartMasterTbl>().FindAsync(id);
                if (cart == null)
                    return NotFound("Cart item not found");

                _context.Set<cartMasterTbl>().Remove(cart);
                await _context.SaveChangesAsync();

                return Ok("Cart item removed successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
    }
}

using auto.ecom.api.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace auto.ecom.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly AutoDbContext _context;

        public CategoryController(AutoDbContext context)
        {
            _context = context;
        }

        [HttpGet("getAllCategory")]
        public IActionResult GetCategories()
        {
            var categories = _context.CategoryMasters.ToList();
            return Ok(categories);
        }

        [HttpPost("SaveCategory")]
        public IActionResult SaveCategory([FromBody] CategoryMaster obj)
        {
            _context.CategoryMasters.Add(obj);
            _context.SaveChanges();
            return Ok(obj);
        }

        [HttpPut("UpdateCategory")]
        public IActionResult UpdateCategory([FromBody] CategoryMaster obj)
        {
            var existingCategory = _context.CategoryMasters.Find(obj.categoryId);
            if (existingCategory == null)
            {
                return NotFound("Category not found");
            }
            else
            {
                existingCategory.imageName = obj.imageName;
                existingCategory.categoryName = obj.categoryName;
                return Ok(obj);
            }
        }


        [HttpGet("DeleteCategoryById")]
        public IActionResult DeleteCategoryById(int id)
        {
            var existingCategory = _context.CategoryMasters.Find(id);
            if (existingCategory == null)
            {
                return NotFound("Category not found");
            }
            else
            {
                _context.CategoryMasters.Remove(existingCategory);
                _context.SaveChanges();
                return Ok("Category Deleted Success");
            }
        }


        [HttpPost("add")]
        public async Task<IActionResult> AddChildCategory([FromBody] ChildCategoryMaster model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Check duplicate name globally
                bool nameExists = await _context.ChildCategoryMasters
                    .AnyAsync(x => x.childCategoryName.Trim().ToLower() == model.childCategoryName.Trim().ToLower());

                if (nameExists)
                    return BadRequest("Child category name already exists.");

                // Check duplicate in same category
                bool categoryNameExists = await _context.ChildCategoryMasters
                    .AnyAsync(x => x.categoryId == model.categoryId &&
                                   x.childCategoryName.Trim().ToLower() == model.childCategoryName.Trim().ToLower());

                if (categoryNameExists)
                    return BadRequest("Child category already exists for this category.");

                _context.ChildCategoryMasters.Add(model);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Child category added successfully.",
                    data = model
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // ============================
        // GET ALL
        // ============================
        [HttpGet("list-with-category")]
        public async Task<IActionResult> GetAllWithCategory()
        {
            try
            {
                var data = await (from cc in _context.ChildCategoryMasters
                                  join c in _context.CategoryMasters on cc.categoryId equals c.categoryId
                                  select new ChildCategoryWithCategoryDto
                                  {
                                      childCategoryId = cc.childCategoryId,
                                      childCategoryName = cc.childCategoryName,
                                      categoryId = cc.categoryId,
                                      categoryName = c.categoryName,
                                      imageName = c.imageName
                                  }).ToListAsync();

                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // ============================
        // GET BY ID
        // ============================
        [HttpGet("get/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var data = await _context.ChildCategoryMasters.FindAsync(id);

                if (data == null)
                    return NotFound("Child category not found.");

                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // ============================
        // UPDATE
        // ============================
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ChildCategoryMaster model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var existing = await _context.ChildCategoryMasters.FindAsync(id);
                if (existing == null)
                    return NotFound("Child category not found.");

                // Duplicate name check (ignore current id)
                bool nameExists = await _context.ChildCategoryMasters
                    .AnyAsync(x => x.childCategoryName.Trim().ToLower() == model.childCategoryName.Trim().ToLower() &&
                                   x.childCategoryId != id);

                if (nameExists)
                    return BadRequest("Child category name already exists.");

                // Duplicate in same category
                bool categoryNameExists = await _context.ChildCategoryMasters
                    .AnyAsync(x => x.categoryId == model.categoryId &&
                                   x.childCategoryName.Trim().ToLower() == model.childCategoryName.Trim().ToLower() &&
                                   x.childCategoryId != id);

                if (categoryNameExists)
                    return BadRequest("Child category already exists for this category.");

                existing.childCategoryName = model.childCategoryName;
                existing.categoryId = model.categoryId;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Child category updated successfully.",
                    data = existing
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // ============================
        // DELETE
        // ============================
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var existing = await _context.ChildCategoryMasters.FindAsync(id);
                if (existing == null)
                    return NotFound("Child category not found.");

                _context.ChildCategoryMasters.Remove(existing);
                await _context.SaveChangesAsync();

                return Ok("Child category deleted successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using HealthcareApp.Models;
using HealthcareApp.Data;

namespace HealthcareApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly ApplicationDbContext _context;

        public AccountController(ILogger<AccountController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // GET
        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            var model = new LoginViewModel { ReturnUrl = returnUrl };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = _context.Users.FirstOrDefault(u =>
                u.Email.Trim().ToLower() == model.Username.Trim().ToLower()
                && u.Password == model.Password);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid username or password");
                return View(model);
            }

            // Session set
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserName", user.FullName);
            HttpContext.Session.SetString("UserRoleId", user.RoleId.ToString());

            Console.WriteLine("Login Success: " + user.Email);

            // ✅ Redirect
            if (user.RoleId == 2)
                return RedirectToAction("Index", "Doctor");

            else if (user.RoleId == 1)
                return RedirectToAction("Index", "Admin");

            else
                return RedirectToAction("Index", "Dashboard");
        }

        // Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // Protected page example
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("UserEmail") == null)
            {
                return RedirectToAction("Login");
            }

            return View();
        }
    }
}

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



using ECommerceApp.Models;

namespace ECommerceApp.Data
{
    public class ProductRepository
    {
        public List<Product> GetAllProducts()
        {
            return new List<Product>
            {
                new Product { Id = 1, Name = "Laptop", Price = 50000, Description="Good laptop", ImageUrl="/images/laptop.jpg"},
                new Product { Id = 2, Name = "Phone", Price = 20000, Description="Smart phone", ImageUrl="/images/phone.jpg"},
                new Product { Id = 3, Name = "Headphones", Price = 3000, Description="Wireless", ImageUrl="/images/headphones.jpg"}
            };
        }

        public Product GetProductById(int id)
        {
            return GetAllProducts().FirstOrDefault(p => p.Id == id);
        }
    }
}


using Microsoft.AspNetCore.Mvc;
using ECommerceApp.Data;

namespace ECommerceApp.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ProductRepository _repo;

        public ProductsController()
        {
            _repo = new ProductRepository();
        }

        public IActionResult Index()
        {
            var products = _repo.GetAllProducts();
            return View(products);
        }

        public IActionResult Details(int id)
        {
            var product = _repo.GetProductById(id);
            if (product == null) return NotFound();

            return View(product);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using ECommerceApp.Data;
using ECommerceApp.Models;
using System.Text.Json;

namespace ECommerceApp.Controllers
{
    public class CartController : Controller
    {
        private readonly ProductRepository _repo;

        public CartController()
        {
            _repo = new ProductRepository();
        }

        public IActionResult Index()
        {
            var cart = GetCart();
            return View(cart);
        }

        [HttpPost]
        public IActionResult AddToCart(int productId)
        {
            var product = _repo.GetProductById(productId);

            var cart = GetCart();
            cart.Add(product);

            SaveCart(cart);

            return RedirectToAction("Index");
        }

        private List<Product> GetCart()
        {
            var sessionData = HttpContext.Session.GetString("Cart");

            if (sessionData == null)
                return new List<Product>();

            return JsonSerializer.Deserialize<List<Product>>(sessionData);
        }

        private void SaveCart(List<Product> cart)
        {
            HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(cart));
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using ECommerceApp.Models;
using ECommerceApp.Data;
using System.Text.Json;

public class CartController : Controller
{
    private readonly ProductRepository _repo = new ProductRepository();

    public IActionResult Index()
    {
        return View(GetCart());
    }

    [HttpPost]
    public IActionResult AddToCart(int productId)
    {
        var product = _repo.GetProductById(productId);
        var cart = GetCart();

        var existing = cart.FirstOrDefault(c => c.Product.Id == productId);

        if (existing != null)
            existing.Quantity++;
        else
            cart.Add(new CartItem { Product = product, Quantity = 1 });

        SaveCart(cart);

        return RedirectToAction("Index");
    }

    public IActionResult Remove(int id)
    {
        var cart = GetCart();
        var item = cart.FirstOrDefault(c => c.Product.Id == id);

        if (item != null)
            cart.Remove(item);

        SaveCart(cart);
        return RedirectToAction("Index");
    }

    private List<CartItem> GetCart()
    {
        var data = HttpContext.Session.GetString("Cart");
        return data == null
            ? new List<CartItem>()
            : JsonSerializer.Deserialize<List<CartItem>>(data);
    }

    private void SaveCart(List<CartItem> cart)
    {
        HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(cart));
    }
}


using Microsoft.AspNetCore.Mvc;
using ECommerceApp.Models;
using System.Text.Json;

public class CheckoutController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public IActionResult PlaceOrder(string name, string address)
    {
        // In real app → save to DB

        HttpContext.Session.Remove("Cart");

        return RedirectToAction("Success");
    }

    public IActionResult Success()
    {
        return View();
    }
}

using Microsoft.AspNetCore.Mvc;
using ECommerceApp.Models;
using ECommerceApp.Data;

public class AdminController : Controller
{
    private static List<Product> products = new ProductRepository().GetAllProducts();

    public IActionResult Index()
    {
        return View(products);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Create(Product p)
    {
        p.Id = products.Max(x => x.Id) + 1;
        products.Add(p);
        return RedirectToAction("Index");
    }

    public IActionResult Edit(int id)
    {
        var product = products.FirstOrDefault(x => x.Id == id);
        return View(product);
    }

    [HttpPost]
    public IActionResult Edit(Product p)
    {
        var existing = products.First(x => x.Id == p.Id);
        existing.Name = p.Name;
        existing.Price = p.Price;
        existing.Description = p.Description;

        return RedirectToAction("Index");
    }

    public IActionResult Delete(int id)
    {
        var product = products.First(x => x.Id == id);
        products.Remove(product);

        return RedirectToAction("Index");
    }
}



/* Demo Code*/

using auto.ecom.api.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace auto.ecom.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartMasterController : Controller
    {
        private readonly AutoDbContext _context;

        public CartMasterController(AutoDbContext context)
        {
            _context = context;
        }
        [HttpGet("GetALL")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await (
                                    from cmt in _context.cartMasterTbls
                                    join pm in _context.productMasters on cmt.productId equals pm.productId
                                    join cm in _context.Customers on cmt.customerId equals cm.custId
                                    select new
                                    {
                                        cmt.quantity,
                                        cmt.addedDate,
                                        cmt.modifiedDate,
                                        pm.shortName,
                                        pm.fullName,
                                        pm.price,
                                        pm.description,
                                        pm.thumbnailImage,
                                        pm.productImage,
                                        pm.sku,
                                        cm.customerName,
                                        cm.mobileNo,
                                        cm.email,
                                        cm.city,
                                        cm.address,
                                        cm.pincode
                                    }).ToListAsync();

                //var data = await _context.Set<cartMasterTbl>().ToListAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // GET CART ITEM BY ID
        [HttpGet("GetCartItemById{id}")]
        public async Task<IActionResult> GetCartItemById(int id)
        {
            try
            {
                var cart = await (
                                    from cmt in _context.cartMasterTbls
                                    join pm in _context.productMasters on cmt.productId equals pm.productId
                                    //join cm in _context.Customers on cmt.customerId equals cm.custId
                                    where cmt.cartId == id
                                    select new
                                    {
                                        cmt.quantity,
                                        cmt.addedDate,
                                        cmt.modifiedDate,
                                        pm.shortName,
                                        pm.fullName,
                                        pm.price,
                                        pm.description,
                                        pm.thumbnailImage,
                                        pm.productImage,
                                        pm.sku,
                                        //cm.customerName,
                                        //cm.mobileNo,
                                        //cm.email,
                                        // cm.city,
                                        //cm.address,
                                        // cm.pincode
                                    }).ToListAsync();

                //var cart = await _context.Set<cartMasterTbl>().FindAsync(id);
                if (cart == null)
                    return NotFound("Cart item not found");

                return Ok(cart);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // ADD TO CART
        [HttpPost("AddToCart")]
        public async Task<IActionResult> Create(cartMasterTbl model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                model.addedDate = DateTime.Now;
                model.modifiedDate = DateTime.Now;

                await _context.Set<cartMasterTbl>().AddAsync(model);
                await _context.SaveChangesAsync();

                return Ok("Item added to cart successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // UPDATE CART ITEM
        [HttpPut("UpdateCartItemById/{id}")]
        public async Task<IActionResult> Update(int id, cartMasterTbl model)
        {
            try
            {
                var existing = await _context.Set<cartMasterTbl>().FindAsync(id);
                if (existing == null)
                    return NotFound("Cart item not found");

                model.modifiedDate = DateTime.Now;
                _context.Entry(existing).CurrentValues.SetValues(model);
                await _context.SaveChangesAsync();

                return Ok("Cart item updated successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // DELETE CART ITEM
        [HttpDelete("DeleteCartItemById/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var cart = await _context.Set<cartMasterTbl>().FindAsync(id);
                if (cart == null)
                    return NotFound("Cart item not found");

                _context.Set<cartMasterTbl>().Remove(cart);
                await _context.SaveChangesAsync();

                return Ok("Cart item removed successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
    }
}

using auto.ecom.api.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace auto.ecom.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly AutoDbContext _context;

        public CategoryController(AutoDbContext context)
        {
            _context = context;
        }

        [HttpGet("getAllCategory")]
        public IActionResult GetCategories()
        {
            var categories = _context.CategoryMasters.ToList();
            return Ok(categories);
        }

        [HttpPost("SaveCategory")]
        public IActionResult SaveCategory([FromBody] CategoryMaster obj)
        {
            _context.CategoryMasters.Add(obj);
            _context.SaveChanges();
            return Ok(obj);
        }

        [HttpPut("UpdateCategory")]
        public IActionResult UpdateCategory([FromBody] CategoryMaster obj)
        {
            var existingCategory = _context.CategoryMasters.Find(obj.categoryId);
            if (existingCategory == null)
            {
                return NotFound("Category not found");
            }
            else
            {
                existingCategory.imageName = obj.imageName;
                existingCategory.categoryName = obj.categoryName;
                return Ok(obj);
            }
        }


        [HttpGet("DeleteCategoryById")]
        public IActionResult DeleteCategoryById(int id)
        {
            var existingCategory = _context.CategoryMasters.Find(id);
            if (existingCategory == null)
            {
                return NotFound("Category not found");
            }
            else
            {
                _context.CategoryMasters.Remove(existingCategory);
                _context.SaveChanges();
                return Ok("Category Deleted Success");
            }
        }


        [HttpPost("add")]
        public async Task<IActionResult> AddChildCategory([FromBody] ChildCategoryMaster model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Check duplicate name globally
                bool nameExists = await _context.ChildCategoryMasters
                    .AnyAsync(x => x.childCategoryName.Trim().ToLower() == model.childCategoryName.Trim().ToLower());

                if (nameExists)
                    return BadRequest("Child category name already exists.");

                // Check duplicate in same category
                bool categoryNameExists = await _context.ChildCategoryMasters
                    .AnyAsync(x => x.categoryId == model.categoryId &&
                                   x.childCategoryName.Trim().ToLower() == model.childCategoryName.Trim().ToLower());

                if (categoryNameExists)
                    return BadRequest("Child category already exists for this category.");

                _context.ChildCategoryMasters.Add(model);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Child category added successfully.",
                    data = model
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // ============================
        // GET ALL
        // ============================
        [HttpGet("list-with-category")]
        public async Task<IActionResult> GetAllWithCategory()
        {
            try
            {
                var data = await (from cc in _context.ChildCategoryMasters
                                  join c in _context.CategoryMasters on cc.categoryId equals c.categoryId
                                  select new ChildCategoryWithCategoryDto
                                  {
                                      childCategoryId = cc.childCategoryId,
                                      childCategoryName = cc.childCategoryName,
                                      categoryId = cc.categoryId,
                                      categoryName = c.categoryName,
                                      imageName = c.imageName
                                  }).ToListAsync();

                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // ============================
        // GET BY ID
        // ============================
        [HttpGet("get/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var data = await _context.ChildCategoryMasters.FindAsync(id);

                if (data == null)
                    return NotFound("Child category not found.");

                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // ============================
        // UPDATE
        // ============================
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ChildCategoryMaster model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var existing = await _context.ChildCategoryMasters.FindAsync(id);
                if (existing == null)
                    return NotFound("Child category not found.");

                // Duplicate name check (ignore current id)
                bool nameExists = await _context.ChildCategoryMasters
                    .AnyAsync(x => x.childCategoryName.Trim().ToLower() == model.childCategoryName.Trim().ToLower() &&
                                   x.childCategoryId != id);

                if (nameExists)
                    return BadRequest("Child category name already exists.");

                // Duplicate in same category
                bool categoryNameExists = await _context.ChildCategoryMasters
                    .AnyAsync(x => x.categoryId == model.categoryId &&
                                   x.childCategoryName.Trim().ToLower() == model.childCategoryName.Trim().ToLower() &&
                                   x.childCategoryId != id);

                if (categoryNameExists)
                    return BadRequest("Child category already exists for this category.");

                existing.childCategoryName = model.childCategoryName;
                existing.categoryId = model.categoryId;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Child category updated successfully.",
                    data = existing
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // ============================
        // DELETE
        // ============================
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var existing = await _context.ChildCategoryMasters.FindAsync(id);
                if (existing == null)
                    return NotFound("Child category not found.");

                _context.ChildCategoryMasters.Remove(existing);
                await _context.SaveChangesAsync();

                return Ok("Child category deleted successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}

///


using auto.ecom.api.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace auto.ecom.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderDetailController : Controller
    {
        private readonly AutoDbContext _context;

        public OrderDetailController(AutoDbContext context)
        {
            _context = context;
        }

        // GET ALL ORDER DETAILS
        [HttpGet("GetAllOrders")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var list = await (
                    from ot in _context.orderDetailTabls
                    join pm in _context.productMasters on ot.productId equals pm.productId
                    join om in _context.orderMasters on ot.orderId equals om.orderId
                    select new
                    {
                        ot.quantity,
                        pm.shortName,
                        pm.fullName,
                        pm.price,
                        pm.description,
                        pm.thumbnailImage,
                        pm.productImage,
                        pm.sku,
                        om.orderDate,
                        om.totalAmout,
                        om.paymentNaration
                    }).ToListAsync();
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // GET BY ORDER DETAIL ID
        [HttpGet("GetOrderDetailsById{id}")]
        public async Task<IActionResult> GetOrderDetailsById(int id)
        {
            try
            {
                var details = await (
                    from ot in _context.orderDetailTabls
                    join pm in _context.productMasters on ot.productId equals pm.productId
                    join om in _context.orderMasters on ot.orderId equals om.orderId
                    where ot.orderDetailId == id
                    select new
                    {
                        ot.quantity,
                        pm.shortName,
                        pm.fullName,
                        pm.price,
                        pm.description,
                        pm.thumbnailImage,
                        pm.productImage,
                        pm.sku,
                        om.orderDate,
                        om.totalAmout,
                        om.paymentNaration
                    }).ToListAsync();

                if (details == null || details.Count == 0)
                    return NotFound("No order details found for this order.");

                return Ok(details);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // CREATE ORDER DETAIL (Add Product to Order)
        [HttpPost("AddItem")]
        public async Task<IActionResult> AddOrderItem(orderDetailTabl model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Prevent duplicate product entry inside the same order
                var exists = await _context.Set<orderDetailTabl>()
                    .AnyAsync(x => x.orderId == model.orderId && x.productId == model.productId);

                if (exists)
                    return BadRequest("Product already added to this order. Update quantity instead!");

                await _context.Set<orderDetailTabl>().AddAsync(model);
                await _context.SaveChangesAsync();

                return Ok("Product added to order successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // UPDATE ITEM QUANTITY
        [HttpPut("UpdateQuantity/{id}")]
        public async Task<IActionResult> UpdateQuantity(int id, int quantity)
        {
            try
            {
                var orderDetail = await _context.Set<orderDetailTabl>().FindAsync(id);
                if (orderDetail == null)
                    return NotFound("Order detail not found.");

                orderDetail.quantity = quantity;
                await _context.SaveChangesAsync();

                return Ok("Quantity updated successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // DELETE AN ORDER ITEM
        [HttpDelete("DeleteOrderItemById{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var detail = await _context.Set<orderDetailTabl>().FindAsync(id);
                if (detail == null)
                    return NotFound("Order detail not found.");

                _context.Set<orderDetailTabl>().Remove(detail);
                await _context.SaveChangesAsync();

                return Ok("Order item removed.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
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
    public class OrderDetailController : Controller
    {
        private readonly AutoDbContext _context;

        public OrderDetailController(AutoDbContext context)
        {
            _context = context;
        }

        // GET ALL ORDER DETAILS
        [HttpGet("GetAllOrders")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var list = await (
                    from ot in _context.orderDetailTabls
                    join pm in _context.productMasters on ot.productId equals pm.productId
                    join om in _context.orderMasters on ot.orderId equals om.orderId
                    select new
                    {
                        ot.quantity,
                        pm.shortName,
                        pm.fullName,
                        pm.price,
                        pm.description,
                        pm.thumbnailImage,
                        pm.productImage,
                        pm.sku,
                        om.orderDate,
                        om.totalAmout,
                        om.paymentNaration
                    }).ToListAsync();
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // GET BY ORDER DETAIL ID
        [HttpGet("GetOrderDetailsById{id}")]
        public async Task<IActionResult> GetOrderDetailsById(int id)
        {
            try
            {
                var details = await (
                    from ot in _context.orderDetailTabls
                    join pm in _context.productMasters on ot.productId equals pm.productId
                    join om in _context.orderMasters on ot.orderId equals om.orderId
                    where ot.orderDetailId == id
                    select new
                    {
                        ot.quantity,
                        pm.shortName,
                        pm.fullName,
                        pm.price,
                        pm.description,
                        pm.thumbnailImage,
                        pm.productImage,
                        pm.sku,
                        om.orderDate,
                        om.totalAmout,
                        om.paymentNaration
                    }).ToListAsync();

                if (details == null || details.Count == 0)
                    return NotFound("No order details found for this order.");

                return Ok(details);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // CREATE ORDER DETAIL (Add Product to Order)
        [HttpPost("AddItem")]
        public async Task<IActionResult> AddOrderItem(orderDetailTabl model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Prevent duplicate product entry inside the same order
                var exists = await _context.Set<orderDetailTabl>()
                    .AnyAsync(x => x.orderId == model.orderId && x.productId == model.productId);

                if (exists)
                    return BadRequest("Product already added to this order. Update quantity instead!");

                await _context.Set<orderDetailTabl>().AddAsync(model);
                await _context.SaveChangesAsync();

                return Ok("Product added to order successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // UPDATE ITEM QUANTITY
        [HttpPut("UpdateQuantity/{id}")]
        public async Task<IActionResult> UpdateQuantity(int id, int quantity)
        {
            try
            {
                var orderDetail = await _context.Set<orderDetailTabl>().FindAsync(id);
                if (orderDetail == null)
                    return NotFound("Order detail not found.");

                orderDetail.quantity = quantity;
                await _context.SaveChangesAsync();

                return Ok("Quantity updated successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // DELETE AN ORDER ITEM
        [HttpDelete("DeleteOrderItemById{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var detail = await _context.Set<orderDetailTabl>().FindAsync(id);
                if (detail == null)
                    return NotFound("Order detail not found.");

                _context.Set<orderDetailTabl>().Remove(detail);
                await _context.SaveChangesAsync();

                return Ok("Order item removed.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
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
    public class productMasterController : Controller
    {
        private readonly AutoDbContext _context;

        public productMasterController(AutoDbContext context)
        {
            _context = context;
        }

        //  GET ALL PRODUCT 
        [HttpGet("GetAllProduct")]
        public IActionResult GetAllProduct()
        {
            try
            {
                var customers = (from pm in _context.productMasters
                                 join ccm in _context.ChildCategoryMasters
                                 on pm.childCategoryId equals ccm.childCategoryId
                                 select new
                                 {
                                     pm.shortName,
                                     pm.fullName,
                                     pm.price,
                                     pm.description,
                                     pm.thumbnailImage,
                                     pm.productImage,
                                     pm.sku,
                                     ccm.childCategoryName
                                 }).ToList();
                //var customers = _context.productMasters.ToList();
                return Ok(customers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // GET BY ID
        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var product1 = await (from pm in _context.productMasters
                                      join cm in _context.ChildCategoryMasters
                                      on pm.childCategoryId equals cm.childCategoryId
                                      where pm.productId == id
                                      select new
                                      {
                                          pm.shortName,
                                          pm.fullName,
                                          pm.price,
                                          pm.description,
                                          pm.thumbnailImage,
                                          pm.productImage,
                                          pm.sku,
                                          cm.childCategoryName
                                      }).FirstOrDefaultAsync();
                // var product = await _context.Set<productMaster>().FindAsync(id);
                if (product1 == null)
                    return NotFound("Product not found");

                return Ok(product1);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // CREATE
        [HttpPost("AddProduct")]
        public async Task<IActionResult> AddProduct(productMaster model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                //  Check for duplicate by SKU (or you can use shortName/fullName)
                var exists = await _context.Set<productMaster>()
                                           .AnyAsync(p => p.sku == model.sku);

                if (exists)
                    return BadRequest("Product with the same SKU already exists!"); // Duplicate message

                // If not duplicate → save product
                await _context.Set<productMaster>().AddAsync(model);
                await _context.SaveChangesAsync();

                return Ok("Product created successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpPut("UpdateProduct/{id}")]
        public async Task<IActionResult> UpdateProduct(int id, productMaster model)
        {
            try
            {
                var existing = await _context.Set<productMaster>().FindAsync(id);
                if (existing == null)
                    return NotFound("Product not found");

                _context.Entry(existing).CurrentValues.SetValues(model);
                await _context.SaveChangesAsync();

                return Ok("Product updated successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // DELETE
        [HttpDelete("DeleteProduct/{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var product = await _context.Set<productMaster>().FindAsync(id);
                if (product == null)
                    return NotFound("Product not found");

                _context.Set<productMaster>().Remove(product);
                await _context.SaveChangesAsync();

                return Ok("Product deleted successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
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



using auto.ecom.api.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace auto.ecom.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderDetailController : Controller
    {
        private readonly AutoDbContext _context;

        public OrderDetailController(AutoDbContext context)
        {
            _context = context;
        }

        // GET ALL ORDER DETAILS
        [HttpGet("GetAllOrders")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var list = await (
                    from ot in _context.orderDetailTabls
                    join pm in _context.productMasters on ot.productId equals pm.productId
                    join om in _context.orderMasters on ot.orderId equals om.orderId
                    select new
                    {
                        ot.quantity,
                        pm.shortName,
                        pm.fullName,
                        pm.price,
                        pm.description,
                        pm.thumbnailImage,
                        pm.productImage,
                        pm.sku,
                        om.orderDate,
                        om.totalAmout,
                        om.paymentNaration
                    }).ToListAsync();
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // GET BY ORDER DETAIL ID
        [HttpGet("GetOrderDetailsById{id}")]
        public async Task<IActionResult> GetOrderDetailsById(int id)
        {
            try
            {
                var details = await (
                    from ot in _context.orderDetailTabls
                    join pm in _context.productMasters on ot.productId equals pm.productId
                    join om in _context.orderMasters on ot.orderId equals om.orderId
                    where ot.orderDetailId == id
                    select new
                    {
                        ot.quantity,
                        pm.shortName,
                        pm.fullName,
                        pm.price,
                        pm.description,
                        pm.thumbnailImage,
                        pm.productImage,
                        pm.sku,
                        om.orderDate,
                        om.totalAmout,
                        om.paymentNaration
                    }).ToListAsync();

                if (details == null || details.Count == 0)
                    return NotFound("No order details found for this order.");

                return Ok(details);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // CREATE ORDER DETAIL (Add Product to Order)
        [HttpPost("AddItem")]
        public async Task<IActionResult> AddOrderItem(orderDetailTabl model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Prevent duplicate product entry inside the same order
                var exists = await _context.Set<orderDetailTabl>()
                    .AnyAsync(x => x.orderId == model.orderId && x.productId == model.productId);

                if (exists)
                    return BadRequest("Product already added to this order. Update quantity instead!");

                await _context.Set<orderDetailTabl>().AddAsync(model);
                await _context.SaveChangesAsync();

                return Ok("Product added to order successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // UPDATE ITEM QUANTITY
        [HttpPut("UpdateQuantity/{id}")]
        public async Task<IActionResult> UpdateQuantity(int id, int quantity)
        {
            try
            {
                var orderDetail = await _context.Set<orderDetailTabl>().FindAsync(id);
                if (orderDetail == null)
                    return NotFound("Order detail not found.");

                orderDetail.quantity = quantity;
                await _context.SaveChangesAsync();

                return Ok("Quantity updated successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // DELETE AN ORDER ITEM
        [HttpDelete("DeleteOrderItemById{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var detail = await _context.Set<orderDetailTabl>().FindAsync(id);
                if (detail == null)
                    return NotFound("Order detail not found.");

                _context.Set<orderDetailTabl>().Remove(detail);
                await _context.SaveChangesAsync();

                return Ok("Order item removed.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
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
    public class OrderDetailController : Controller
    {
        private readonly AutoDbContext _context;

        public OrderDetailController(AutoDbContext context)
        {
            _context = context;
        }

        // GET ALL ORDER DETAILS
        [HttpGet("GetAllOrders")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var list = await (
                    from ot in _context.orderDetailTabls
                    join pm in _context.productMasters on ot.productId equals pm.productId
                    join om in _context.orderMasters on ot.orderId equals om.orderId
                    select new
                    {
                        ot.quantity,
                        pm.shortName,
                        pm.fullName,
                        pm.price,
                        pm.description,
                        pm.thumbnailImage,
                        pm.productImage,
                        pm.sku,
                        om.orderDate,
                        om.totalAmout,
                        om.paymentNaration
                    }).ToListAsync();
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // GET BY ORDER DETAIL ID
        [HttpGet("GetOrderDetailsById{id}")]
        public async Task<IActionResult> GetOrderDetailsById(int id)
        {
            try
            {
                var details = await (
                    from ot in _context.orderDetailTabls
                    join pm in _context.productMasters on ot.productId equals pm.productId
                    join om in _context.orderMasters on ot.orderId equals om.orderId
                    where ot.orderDetailId == id
                    select new
                    {
                        ot.quantity,
                        pm.shortName,
                        pm.fullName,
                        pm.price,
                        pm.description,
                        pm.thumbnailImage,
                        pm.productImage,
                        pm.sku,
                        om.orderDate,
                        om.totalAmout,
                        om.paymentNaration
                    }).ToListAsync();

                if (details == null || details.Count == 0)
                    return NotFound("No order details found for this order.");

                return Ok(details);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // CREATE ORDER DETAIL (Add Product to Order)
        [HttpPost("AddItem")]
        public async Task<IActionResult> AddOrderItem(orderDetailTabl model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Prevent duplicate product entry inside the same order
                var exists = await _context.Set<orderDetailTabl>()
                    .AnyAsync(x => x.orderId == model.orderId && x.productId == model.productId);

                if (exists)
                    return BadRequest("Product already added to this order. Update quantity instead!");

                await _context.Set<orderDetailTabl>().AddAsync(model);
                await _context.SaveChangesAsync();

                return Ok("Product added to order successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // UPDATE ITEM QUANTITY
        [HttpPut("UpdateQuantity/{id}")]
        public async Task<IActionResult> UpdateQuantity(int id, int quantity)
        {
            try
            {
                var orderDetail = await _context.Set<orderDetailTabl>().FindAsync(id);
                if (orderDetail == null)
                    return NotFound("Order detail not found.");

                orderDetail.quantity = quantity;
                await _context.SaveChangesAsync();

                return Ok("Quantity updated successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // DELETE AN ORDER ITEM
        [HttpDelete("DeleteOrderItemById{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var detail = await _context.Set<orderDetailTabl>().FindAsync(id);
                if (detail == null)
                    return NotFound("Order detail not found.");

                _context.Set<orderDetailTabl>().Remove(detail);
                await _context.SaveChangesAsync();

                return Ok("Order item removed.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
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
    public class productMasterController : Controller
    {
        private readonly AutoDbContext _context;

        public productMasterController(AutoDbContext context)
        {
            _context = context;
        }

        //  GET ALL PRODUCT 
        [HttpGet("GetAllProduct")]
        public IActionResult GetAllProduct()
        {
            try
            {
                var customers = (from pm in _context.productMasters
                                 join ccm in _context.ChildCategoryMasters
                                 on pm.childCategoryId equals ccm.childCategoryId
                                 select new
                                 {
                                     pm.shortName,
                                     pm.fullName,
                                     pm.price,
                                     pm.description,
                                     pm.thumbnailImage,
                                     pm.productImage,
                                     pm.sku,
                                     ccm.childCategoryName
                                 }).ToList();
                //var customers = _context.productMasters.ToList();
                return Ok(customers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // GET BY ID
        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var product1 = await (from pm in _context.productMasters
                                      join cm in _context.ChildCategoryMasters
                                      on pm.childCategoryId equals cm.childCategoryId
                                      where pm.productId == id
                                      select new
                                      {
                                          pm.shortName,
                                          pm.fullName,
                                          pm.price,
                                          pm.description,
                                          pm.thumbnailImage,
                                          pm.productImage,
                                          pm.sku,
                                          cm.childCategoryName
                                      }).FirstOrDefaultAsync();
                // var product = await _context.Set<productMaster>().FindAsync(id);
                if (product1 == null)
                    return NotFound("Product not found");

                return Ok(product1);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // CREATE
        [HttpPost("AddProduct")]
        public async Task<IActionResult> AddProduct(productMaster model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                //  Check for duplicate by SKU (or you can use shortName/fullName)
                var exists = await _context.Set<productMaster>()
                                           .AnyAsync(p => p.sku == model.sku);

                if (exists)
                    return BadRequest("Product with the same SKU already exists!"); // Duplicate message

                // If not duplicate → save product
                await _context.Set<productMaster>().AddAsync(model);
                await _context.SaveChangesAsync();

                return Ok("Product created successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpPut("UpdateProduct/{id}")]
        public async Task<IActionResult> UpdateProduct(int id, productMaster model)
        {
            try
            {
                var existing = await _context.Set<productMaster>().FindAsync(id);
                if (existing == null)
                    return NotFound("Product not found");

                _context.Entry(existing).CurrentValues.SetValues(model);
                await _context.SaveChangesAsync();

                return Ok("Product updated successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // DELETE
        [HttpDelete("DeleteProduct/{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var product = await _context.Set<productMaster>().FindAsync(id);
                if (product == null)
                    return NotFound("Product not found");

                _context.Set<productMaster>().Remove(product);
                await _context.SaveChangesAsync();

                return Ok("Product deleted successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
    }
}



///



using Microsoft.AspNetCore.Mvc;
using HealthcareApp.Models;
using HealthcareApp.Data;

namespace HealthcareApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly ApplicationDbContext _context;

        public AccountController(ILogger<AccountController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // GET
        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            var model = new LoginViewModel { ReturnUrl = returnUrl };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = _context.Users.FirstOrDefault(u =>
                u.Email.Trim().ToLower() == model.Username.Trim().ToLower()
                && u.Password == model.Password);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid username or password");
                return View(model);
            }

            // Session set
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserName", user.FullName);
            HttpContext.Session.SetString("UserRoleId", user.RoleId.ToString());

            Console.WriteLine("Login Success: " + user.Email);

            // ✅ Redirect
            if (user.RoleId == 2)
                return RedirectToAction("Index", "Doctor");

            else if (user.RoleId == 1)
                return RedirectToAction("Index", "Admin");

            else
                return RedirectToAction("Index", "Dashboard");
        }

        // Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // Protected page example
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("UserEmail") == null)
            {
                return RedirectToAction("Login");
            }

            return View();
        }
    }
}


/* Demo Code*/

using auto.ecom.api.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace auto.ecom.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartMasterController : Controller
    {
        private readonly AutoDbContext _context;

        public CartMasterController(AutoDbContext context)
        {
            _context = context;
        }
        [HttpGet("GetALL")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await (
                                    from cmt in _context.cartMasterTbls
                                    join pm in _context.productMasters on cmt.productId equals pm.productId
                                    join cm in _context.Customers on cmt.customerId equals cm.custId
                                    select new
                                    {
                                        cmt.quantity,
                                        cmt.addedDate,
                                        cmt.modifiedDate,
                                        pm.shortName,
                                        pm.fullName,
                                        pm.price,
                                        pm.description,
                                        pm.thumbnailImage,
                                        pm.productImage,
                                        pm.sku,
                                        cm.customerName,
                                        cm.mobileNo,
                                        cm.email,
                                        cm.city,
                                        cm.address,
                                        cm.pincode
                                    }).ToListAsync();

                //var data = await _context.Set<cartMasterTbl>().ToListAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // GET CART ITEM BY ID
        [HttpGet("GetCartItemById{id}")]
        public async Task<IActionResult> GetCartItemById(int id)
        {
            try
            {
                var cart = await (
                                    from cmt in _context.cartMasterTbls
                                    join pm in _context.productMasters on cmt.productId equals pm.productId
                                    //join cm in _context.Customers on cmt.customerId equals cm.custId
                                    where cmt.cartId == id
                                    select new
                                    {
                                        cmt.quantity,
                                        cmt.addedDate,
                                        cmt.modifiedDate,
                                        pm.shortName,
                                        pm.fullName,
                                        pm.price,
                                        pm.description,
                                        pm.thumbnailImage,
                                        pm.productImage,
                                        pm.sku,
                                        //cm.customerName,
                                        //cm.mobileNo,
                                        //cm.email,
                                        // cm.city,
                                        //cm.address,
                                        // cm.pincode
                                    }).ToListAsync();

                //var cart = await _context.Set<cartMasterTbl>().FindAsync(id);
                if (cart == null)
                    return NotFound("Cart item not found");

                return Ok(cart);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // ADD TO CART
        [HttpPost("AddToCart")]
        public async Task<IActionResult> Create(cartMasterTbl model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                model.addedDate = DateTime.Now;
                model.modifiedDate = DateTime.Now;

                await _context.Set<cartMasterTbl>().AddAsync(model);
                await _context.SaveChangesAsync();

                return Ok("Item added to cart successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // UPDATE CART ITEM
        [HttpPut("UpdateCartItemById/{id}")]
        public async Task<IActionResult> Update(int id, cartMasterTbl model)
        {
            try
            {
                var existing = await _context.Set<cartMasterTbl>().FindAsync(id);
                if (existing == null)
                    return NotFound("Cart item not found");

                model.modifiedDate = DateTime.Now;
                _context.Entry(existing).CurrentValues.SetValues(model);
                await _context.SaveChangesAsync();

                return Ok("Cart item updated successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // DELETE CART ITEM
        [HttpDelete("DeleteCartItemById/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var cart = await _context.Set<cartMasterTbl>().FindAsync(id);
                if (cart == null)
                    return NotFound("Cart item not found");

                _context.Set<cartMasterTbl>().Remove(cart);
                await _context.SaveChangesAsync();

                return Ok("Cart item removed successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
    }
}

using auto.ecom.api.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace auto.ecom.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly AutoDbContext _context;

        public CategoryController(AutoDbContext context)
        {
            _context = context;
        }

        [HttpGet("getAllCategory")]
        public IActionResult GetCategories()
        {
            var categories = _context.CategoryMasters.ToList();
            return Ok(categories);
        }

        [HttpPost("SaveCategory")]
        public IActionResult SaveCategory([FromBody] CategoryMaster obj)
        {
            _context.CategoryMasters.Add(obj);
            _context.SaveChanges();
            return Ok(obj);
        }

        [HttpPut("UpdateCategory")]
        public IActionResult UpdateCategory([FromBody] CategoryMaster obj)
        {
            var existingCategory = _context.CategoryMasters.Find(obj.categoryId);
            if (existingCategory == null)
            {
                return NotFound("Category not found");
            }
            else
            {
                existingCategory.imageName = obj.imageName;
                existingCategory.categoryName = obj.categoryName;
                return Ok(obj);
            }
        }


        [HttpGet("DeleteCategoryById")]
        public IActionResult DeleteCategoryById(int id)
        {
            var existingCategory = _context.CategoryMasters.Find(id);
            if (existingCategory == null)
            {
                return NotFound("Category not found");
            }
            else
            {
                _context.CategoryMasters.Remove(existingCategory);
                _context.SaveChanges();
                return Ok("Category Deleted Success");
            }
        }


        [HttpPost("add")]
        public async Task<IActionResult> AddChildCategory([FromBody] ChildCategoryMaster model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Check duplicate name globally
                bool nameExists = await _context.ChildCategoryMasters
                    .AnyAsync(x => x.childCategoryName.Trim().ToLower() == model.childCategoryName.Trim().ToLower());

                if (nameExists)
                    return BadRequest("Child category name already exists.");

                // Check duplicate in same category
                bool categoryNameExists = await _context.ChildCategoryMasters
                    .AnyAsync(x => x.categoryId == model.categoryId &&
                                   x.childCategoryName.Trim().ToLower() == model.childCategoryName.Trim().ToLower());

                if (categoryNameExists)
                    return BadRequest("Child category already exists for this category.");

                _context.ChildCategoryMasters.Add(model);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Child category added successfully.",
                    data = model
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // ============================
        // GET ALL
        // ============================
        [HttpGet("list-with-category")]
        public async Task<IActionResult> GetAllWithCategory()
        {
            try
            {
                var data = await (from cc in _context.ChildCategoryMasters
                                  join c in _context.CategoryMasters on cc.categoryId equals c.categoryId
                                  select new ChildCategoryWithCategoryDto
                                  {
                                      childCategoryId = cc.childCategoryId,
                                      childCategoryName = cc.childCategoryName,
                                      categoryId = cc.categoryId,
                                      categoryName = c.categoryName,
                                      imageName = c.imageName
                                  }).ToListAsync();

                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // ============================
        // GET BY ID
        // ============================
        [HttpGet("get/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var data = await _context.ChildCategoryMasters.FindAsync(id);

                if (data == null)
                    return NotFound("Child category not found.");

                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // ============================
        // UPDATE
        // ============================
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ChildCategoryMaster model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var existing = await _context.ChildCategoryMasters.FindAsync(id);
                if (existing == null)
                    return NotFound("Child category not found.");

                // Duplicate name check (ignore current id)
                bool nameExists = await _context.ChildCategoryMasters
                    .AnyAsync(x => x.childCategoryName.Trim().ToLower() == model.childCategoryName.Trim().ToLower() &&
                                   x.childCategoryId != id);

                if (nameExists)
                    return BadRequest("Child category name already exists.");

                // Duplicate in same category
                bool categoryNameExists = await _context.ChildCategoryMasters
                    .AnyAsync(x => x.categoryId == model.categoryId &&
                                   x.childCategoryName.Trim().ToLower() == model.childCategoryName.Trim().ToLower() &&
                                   x.childCategoryId != id);

                if (categoryNameExists)
                    return BadRequest("Child category already exists for this category.");

                existing.childCategoryName = model.childCategoryName;
                existing.categoryId = model.categoryId;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Child category updated successfully.",
                    data = existing
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // ============================
        // DELETE
        // ============================
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var existing = await _context.ChildCategoryMasters.FindAsync(id);
                if (existing == null)
                    return NotFound("Child category not found.");

                _context.ChildCategoryMasters.Remove(existing);
                await _context.SaveChangesAsync();

                return Ok("Child category deleted successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using HealthcareApp.Models;
using HealthcareApp.Data;

namespace HealthcareApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly ApplicationDbContext _context;

        public AccountController(ILogger<AccountController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // GET
        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            var model = new LoginViewModel { ReturnUrl = returnUrl };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = _context.Users.FirstOrDefault(u =>
                u.Email.Trim().ToLower() == model.Username.Trim().ToLower()
                && u.Password == model.Password);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid username or password");
                return View(model);
            }

            // Session set
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserName", user.FullName);
            HttpContext.Session.SetString("UserRoleId", user.RoleId.ToString());

            Console.WriteLine("Login Success: " + user.Email);

            // ✅ Redirect
            if (user.RoleId == 2)
                return RedirectToAction("Index", "Doctor");

            else if (user.RoleId == 1)
                return RedirectToAction("Index", "Admin");

            else
                return RedirectToAction("Index", "Dashboard");
        }

        // Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // Protected page example
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("UserEmail") == null)
            {
                return RedirectToAction("Login");
            }

            return View();
        }
    }
}

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



using ECommerceApp.Models;

namespace ECommerceApp.Data
{
    public class ProductRepository
    {
        public List<Product> GetAllProducts()
        {
            return new List<Product>
            {
                new Product { Id = 1, Name = "Laptop", Price = 50000, Description="Good laptop", ImageUrl="/images/laptop.jpg"},
                new Product { Id = 2, Name = "Phone", Price = 20000, Description="Smart phone", ImageUrl="/images/phone.jpg"},
                new Product { Id = 3, Name = "Headphones", Price = 3000, Description="Wireless", ImageUrl="/images/headphones.jpg"}
            };
        }

        public Product GetProductById(int id)
        {
            return GetAllProducts().FirstOrDefault(p => p.Id == id);
        }
    }
}


using Microsoft.AspNetCore.Mvc;
using ECommerceApp.Data;

namespace ECommerceApp.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ProductRepository _repo;

        public ProductsController()
        {
            _repo = new ProductRepository();
        }

        public IActionResult Index()
        {
            var products = _repo.GetAllProducts();
            return View(products);
        }

        public IActionResult Details(int id)
        {
            var product = _repo.GetProductById(id);
            if (product == null) return NotFound();

            return View(product);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using ECommerceApp.Data;
using ECommerceApp.Models;
using System.Text.Json;

namespace ECommerceApp.Controllers
{
    public class CartController : Controller
    {
        private readonly ProductRepository _repo;

        public CartController()
        {
            _repo = new ProductRepository();
        }

        public IActionResult Index()
        {
            var cart = GetCart();
            return View(cart);
        }

        [HttpPost]
        public IActionResult AddToCart(int productId)
        {
            var product = _repo.GetProductById(productId);

            var cart = GetCart();
            cart.Add(product);

            SaveCart(cart);

            return RedirectToAction("Index");
        }

        private List<Product> GetCart()
        {
            var sessionData = HttpContext.Session.GetString("Cart");

            if (sessionData == null)
                return new List<Product>();

            return JsonSerializer.Deserialize<List<Product>>(sessionData);
        }

        private void SaveCart(List<Product> cart)
        {
            HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(cart));
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using ECommerceApp.Models;
using ECommerceApp.Data;
using System.Text.Json;

public class CartController : Controller
{
    private readonly ProductRepository _repo = new ProductRepository();

    public IActionResult Index()
    {
        return View(GetCart());
    }

    [HttpPost]
    public IActionResult AddToCart(int productId)
    {
        var product = _repo.GetProductById(productId);
        var cart = GetCart();

        var existing = cart.FirstOrDefault(c => c.Product.Id == productId);

        if (existing != null)
            existing.Quantity++;
        else
            cart.Add(new CartItem { Product = product, Quantity = 1 });

        SaveCart(cart);

        return RedirectToAction("Index");
    }

    public IActionResult Remove(int id)
    {
        var cart = GetCart();
        var item = cart.FirstOrDefault(c => c.Product.Id == id);

        if (item != null)
            cart.Remove(item);

        SaveCart(cart);
        return RedirectToAction("Index");
    }

    private List<CartItem> GetCart()
    {
        var data = HttpContext.Session.GetString("Cart");
        return data == null
            ? new List<CartItem>()
            : JsonSerializer.Deserialize<List<CartItem>>(data);
    }

    private void SaveCart(List<CartItem> cart)
    {
        HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(cart));
    }
}


using Microsoft.AspNetCore.Mvc;
using ECommerceApp.Models;
using System.Text.Json;

public class CheckoutController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public IActionResult PlaceOrder(string name, string address)
    {
        // In real app → save to DB

        HttpContext.Session.Remove("Cart");

        return RedirectToAction("Success");
    }

    public IActionResult Success()
    {
        return View();
    }
}

using Microsoft.AspNetCore.Mvc;
using ECommerceApp.Models;
using ECommerceApp.Data;

public class AdminController : Controller
{
    private static List<Product> products = new ProductRepository().GetAllProducts();

    public IActionResult Index()
    {
        return View(products);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Create(Product p)
    {
        p.Id = products.Max(x => x.Id) + 1;
        products.Add(p);
        return RedirectToAction("Index");
    }

    public IActionResult Edit(int id)
    {
        var product = products.FirstOrDefault(x => x.Id == id);
        return View(product);
    }

    [HttpPost]
    public IActionResult Edit(Product p)
    {
        var existing = products.First(x => x.Id == p.Id);
        existing.Name = p.Name;
        existing.Price = p.Price;
        existing.Description = p.Description;

        return RedirectToAction("Index");
    }

    public IActionResult Delete(int id)
    {
        var product = products.First(x => x.Id == id);
        products.Remove(product);

        return RedirectToAction("Index");
    }
}
