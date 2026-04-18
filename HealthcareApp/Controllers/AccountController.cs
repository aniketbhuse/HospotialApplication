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


