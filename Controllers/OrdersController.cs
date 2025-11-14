using CLDV6212PoePart3.Models;
using CLDV6212PoePart3.Services;
using Microsoft.AspNetCore.Mvc;

namespace CLDV6212PoePart3.Controllers
{
    public class OrdersController : Controller
    {
        private readonly TableStorageService _tableStorageService;

        public OrdersController(TableStorageService tableStorageService)
        {
            _tableStorageService = tableStorageService;
        }

        public async Task<IActionResult> Index()
        {
            var orders = await _tableStorageService.GetAllOrderAsync();
            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> Register()
        {
            ViewData["Customer"] = await _tableStorageService.GetAllCustomersAsync();
            ViewData["Product"] = await _tableStorageService.GetAllProductsAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(Order order)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Customer"] = await _tableStorageService.GetAllCustomersAsync();
                ViewData["Product"] = await _tableStorageService.GetAllProductsAsync();
                return View(order);
            }

            await _tableStorageService.AddOrderAsync(order);
            return RedirectToAction("Index");
        }
    }
}
