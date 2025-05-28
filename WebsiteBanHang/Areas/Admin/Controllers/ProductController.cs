using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using WebsiteBanHang.Models;
using WebsiteBanHang.Repositories;

namespace WebsiteBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)] // Chỉ cho phép Admin truy cập
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public ProductController(IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var products = await _productRepository.GetAllAsync();
                return View(products);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi tải danh sách sản phẩm: " + ex.Message;
                return View(new List<Product>());
            }
        }

        public async Task<IActionResult> Add()
        {
            try
            {
                var categories = await _categoryRepository.GetAllAsync();
                ViewBag.Categories = new SelectList(categories, "Id", "Name");
                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi tải form thêm sản phẩm: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(Product product, IFormFile imageUrl)
        {
            try
            {
                ModelState.Remove("ImageUrl");
                ModelState.Remove("Category");
                ModelState.Remove("Images");

                if (ModelState.IsValid)
                {
                    if (imageUrl != null && imageUrl.Length > 0)
                    {
                        product.ImageUrl = await SaveImage(imageUrl);
                    }

                    await _productRepository.AddAsync(product);
                    TempData["SuccessMessage"] = "Thêm sản phẩm thành công!";
                    return RedirectToAction(nameof(Index));
                }

                var categories = await _categoryRepository.GetAllAsync();
                ViewBag.Categories = new SelectList(categories, "Id", "Name");
                return View(product);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi thêm sản phẩm: " + ex.Message;
                var categories = await _categoryRepository.GetAllAsync();
                ViewBag.Categories = new SelectList(categories, "Id", "Name");
                return View(product);
            }
        }

        public async Task<IActionResult> Display(int id)
        {
            try
            {
                var product = await _productRepository.GetByIdAsync(id);
                if (product == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy sản phẩm!";
                    return RedirectToAction(nameof(Index));
                }
                return View(product);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi tải chi tiết sản phẩm: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Update(int id)
        {
            try
            {
                var product = await _productRepository.GetByIdAsync(id);
                if (product == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy sản phẩm!";
                    return RedirectToAction(nameof(Index));
                }

                var categories = await _categoryRepository.GetAllAsync();
                ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId);
                return View(product);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi tải form cập nhật: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, Product product, IFormFile imageUrl)
        {
            if (id != product.Id)
            {
                TempData["ErrorMessage"] = "ID sản phẩm không hợp lệ!";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                ModelState.Remove("ImageUrl");
                ModelState.Remove("Category");
                ModelState.Remove("Images");

                if (ModelState.IsValid)
                {
                    var existingProduct = await _productRepository.GetByIdAsync(id);
                    if (existingProduct == null)
                    {
                        TempData["ErrorMessage"] = "Không tìm thấy sản phẩm!";
                        return RedirectToAction(nameof(Index));
                    }

                    existingProduct.Name = product.Name;
                    existingProduct.Price = product.Price;
                    existingProduct.Description = product.Description;
                    existingProduct.CategoryId = product.CategoryId;

                    if (imageUrl != null && imageUrl.Length > 0)
                    {
                        existingProduct.ImageUrl = await SaveImage(imageUrl);
                    }

                    await _productRepository.UpdateAsync(existingProduct);
                    TempData["SuccessMessage"] = "Cập nhật sản phẩm thành công!";
                    return RedirectToAction(nameof(Index));
                }

                var categories = await _categoryRepository.GetAllAsync();
                ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId);
                return View(product);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi cập nhật sản phẩm: " + ex.Message;
                var categories = await _categoryRepository.GetAllAsync();
                ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId);
                return View(product);
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var product = await _productRepository.GetByIdAsync(id);
                if (product == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy sản phẩm!";
                    return RedirectToAction(nameof(Index));
                }
                return View(product);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi tải trang xóa sản phẩm: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("DeleteConfirmed")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _productRepository.DeleteAsync(id);
                TempData["SuccessMessage"] = "Xóa sản phẩm thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi xóa sản phẩm: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        private async Task<string> SaveImage(IFormFile image)
        {
            try
            {
                var imagesFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                if (!Directory.Exists(imagesFolder))
                {
                    Directory.CreateDirectory(imagesFolder);
                }

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                var filePath = Path.Combine(imagesFolder, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(fileStream);
                }

                return "/images/" + fileName;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi lưu hình ảnh: " + ex.Message);
            }
        }
    }
}