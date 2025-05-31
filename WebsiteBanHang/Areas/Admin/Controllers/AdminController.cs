// Areas/Admin/Controllers/AdminController.cs - FIX AUTHORIZATION
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WebsiteBanHang.Models;
using WebsiteBanHang.Repositories;

namespace WebsiteBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)] // ✅ QUAN TRỌNG: Thêm dòng này
    public class AdminController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            ILogger<AdminController> logger)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Loading Admin Dashboard");

                var totalProducts = (await _productRepository.GetAllAsync()).Count();
                var totalCategories = (await _categoryRepository.GetAllAsync()).Count();

                ViewBag.TotalProducts = totalProducts;
                ViewBag.TotalCategories = totalCategories;

                _logger.LogInformation($"Dashboard loaded - Products: {totalProducts}, Categories: {totalCategories}");

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Admin Dashboard");
                ViewBag.TotalProducts = 0;
                ViewBag.TotalCategories = 0;
                TempData["ErrorMessage"] = "Lỗi khi tải Dashboard: " + ex.Message;
                return View();
            }
        }
    }
}