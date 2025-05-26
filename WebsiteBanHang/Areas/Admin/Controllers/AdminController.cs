using Microsoft.AspNetCore.Mvc;
using WebsiteBanHang.Models;
using WebsiteBanHang.Repositories;

namespace WebsiteBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public AdminController(IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<IActionResult> Index()
        {
            var totalProducts = (await _productRepository.GetAllAsync()).Count();
            var totalCategories = (await _categoryRepository.GetAllAsync()).Count();

            ViewBag.TotalProducts = totalProducts;
            ViewBag.TotalCategories = totalCategories;

            return View();
        }
    }
}