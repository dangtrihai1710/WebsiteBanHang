using Microsoft.AspNetCore.Mvc;
using WebsiteBanHang.Models;
using WebsiteBanHang.Repositories;
using Microsoft.AspNetCore.Authorization;
namespace WebsiteBanHang.Controllers
{
    [Area ("Admin")]
    [Authorize(Roles = SD.Role_Admin)]// Chỉ cho phép người dùng có vai trò Admin truy cập vào các chức năng này
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public ProductController(IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        // Chỉ giữ lại các chức năng xem cho người dùng
        public async Task<IActionResult> Index()
        {
            var products = await _productRepository.GetAllAsync();
            return View(products);
        }

        public async Task<IActionResult> Display(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // Tìm kiếm sản phẩm
        public async Task<IActionResult> Search(string searchTerm)
        {
            var products = await _productRepository.GetAllAsync();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                products = products.Where(p =>
                    p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    p.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
            }

            ViewBag.SearchTerm = searchTerm;
            return View("Index", products);
        }

        // Lọc theo danh mục
        public async Task<IActionResult> Category(int categoryId)
        {
            var products = await _productRepository.GetAllAsync();
            var category = await _categoryRepository.GetByIdAsync(categoryId);

            if (category == null)
            {
                return NotFound();
            }

            products = products.Where(p => p.CategoryId == categoryId);
            ViewBag.CategoryName = category.Name;

            return View("Index", products);
        }
    }
}