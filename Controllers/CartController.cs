using CLDV6212PoePart3.Data;
using CLDV6212PoePart3.Models;
using CLDV6212PoePart3.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CLDV6212PoePart3.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly TableStorageService _tableStorageService;

        public CartController(ApplicationDbContext context, TableStorageService tableStorageService)
        {
            _context = context;
            _tableStorageService = tableStorageService;
           
        }

        // Show current cart for logged-in customer
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Index", "Login");

            var cart = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.CustomerId == userId && !c.IsOrderPlaced);

            return View(cart ?? new Cart());
        }

        // Add product to cart
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Index", "Login");

            // Validate product exists (from Table Storage)
            var product = await _tableStorageService.GetProductByIdAsync(productId);
            if (product == null)
            {
                TempData["Error"] = "Product not found.";
                return RedirectToAction("Index", "Product");
            }

            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.CustomerId == userId && !c.IsOrderPlaced);

            if (cart == null)
            {
                cart = new Cart { CustomerId = userId.Value };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                cart.Items.Add(new CartItem { ProductId = productId, Quantity = quantity });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Product");
        }

        // Update quantity of a cart item
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
        {
            var item = await _context.CartItems
                .Include(i => i.Cart)
                .FirstOrDefaultAsync(i => i.CartItemId == cartItemId);

            if (item == null) return NotFound();

            if (quantity > 0)
                item.Quantity = quantity;

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // Remove item from cart
        [HttpPost]
        public async Task<IActionResult> RemoveItem(int cartItemId)
        {
            var item = await _context.CartItems.FindAsync(cartItemId);
            if (item == null) return NotFound();

            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // Place order
        [HttpPost]
        public async Task<IActionResult> PlaceOrder(int cartId)
        {
            var cart = await _context.Carts.FindAsync(cartId);
            if (cart == null) return NotFound();

            cart.IsOrderPlaced = true;
            cart.Status = "Pending";

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
