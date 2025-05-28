// Cập nhật file: Controllers/ProductController.cs

using Microsoft.AspNetCore.Mvc;
using WebsiteBanHang.Models;
using WebsiteBanHang.Repositories;

namespace WebsiteBanHang.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public ProductController(IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        // Hiển thị tất cả sản phẩm với filter và search
        public async Task<IActionResult> Index(int? categoryId, string searchTerm, string sortBy = "newest")
        {
            var products = await _productRepository.GetAllAsync();

            // Lọc theo danh mục nếu có
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                products = products.Where(p => p.CategoryId == categoryId.Value);
                var category = await _categoryRepository.GetByIdAsync(categoryId.Value);
                ViewBag.CategoryName = category?.Name;
                ViewBag.CategoryId = categoryId.Value;
            }

            // Tìm kiếm nếu có
            if (!string.IsNullOrEmpty(searchTerm))
            {
                products = products.Where(p =>
                    p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    p.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (p.Category != null && p.Category.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)));
                ViewBag.SearchTerm = searchTerm;
            }

            // Sắp xếp
            products = sortBy switch
            {
                "price-asc" => products.OrderBy(p => p.Price),
                "price-desc" => products.OrderByDescending(p => p.Price),
                "name-asc" => products.OrderBy(p => p.Name),
                "name-desc" => products.OrderByDescending(p => p.Name),
                "newest" => products.OrderByDescending(p => p.Id),
                _ => products.OrderByDescending(p => p.Id)
            };

            ViewBag.SortBy = sortBy;
            return View(products);
        }

        // Hiển thị chi tiết sản phẩm
        public async Task<IActionResult> Display(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // Tìm kiếm sản phẩm (có thể gọi từ AJAX)
        [HttpGet]
        public async Task<IActionResult> Search(string searchTerm, int? categoryId)
        {
            return await Index(categoryId, searchTerm);
        }

        // Lọc theo danh mục (có thể gọi từ AJAX)
        [HttpGet]
        public async Task<IActionResult> Category(int categoryId)
        {
            return await Index(categoryId, null);
        }

        // API để lấy sản phẩm theo danh mục (JSON)
        [HttpGet]
        public async Task<IActionResult> GetProductsByCategory(int categoryId)
        {
            try
            {
                var products = await _productRepository.GetAllAsync();
                var filteredProducts = products.Where(p => p.CategoryId == categoryId);

                var result = filteredProducts.Select(p => new
                {
                    id = p.Id,
                    name = p.Name,
                    price = p.Price,
                    imageUrl = p.ImageUrl,
                    categoryName = p.Category?.Name
                }).ToList();

                return Json(new { success = true, products = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // API để tìm kiếm sản phẩm (JSON)
        [HttpGet]
        public async Task<IActionResult> SearchProducts(string searchTerm)
        {
            try
            {
                var products = await _productRepository.GetAllAsync();

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    products = products.Where(p =>
                        p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        p.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
                }

                var result = products.Select(p => new
                {
                    id = p.Id,
                    name = p.Name,
                    price = p.Price,
                    imageUrl = p.ImageUrl,
                    categoryName = p.Category?.Name
                }).Take(10).ToList(); // Giới hạn 10 kết quả cho autocomplete

                return Json(new { success = true, products = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}