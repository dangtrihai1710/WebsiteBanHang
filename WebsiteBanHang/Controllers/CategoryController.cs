using Microsoft.AspNetCore.Mvc;
using WebsiteBanHang.Models;
using WebsiteBanHang.Repositories;

namespace WebsiteBanHang.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IProductRepository _productRepository;

        public CategoryController(ICategoryRepository categoryRepository, IProductRepository productRepository)
        {
            _categoryRepository = categoryRepository;
            _productRepository = productRepository;
        }

        // Chỉ hiển thị danh sách danh mục cho người dùng xem
        public async Task<IActionResult> Index()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return View(categories);
        }

        // Hiển thị sản phẩm theo danh mục
        public async Task<IActionResult> Products(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            var products = await _productRepository.GetAllAsync();
            products = products.Where(p => p.CategoryId == id);

            ViewBag.CategoryName = category.Name;
            ViewBag.CategoryId = id;

            return View(products);
        }
    }
}