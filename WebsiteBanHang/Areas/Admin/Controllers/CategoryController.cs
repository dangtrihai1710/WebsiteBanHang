﻿// Areas/Admin/Controllers/CategoryController.cs - FIX AUTHORIZATION
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using WebsiteBanHang.Models;
using WebsiteBanHang.Repositories;

namespace WebsiteBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)] // ✅ QUAN TRỌNG: Thêm dòng này
    public class CategoryController : Controller
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IProductRepository _productRepository;
        private readonly ApplicationDbContext _context;

        public CategoryController(ICategoryRepository categoryRepository, IProductRepository productRepository, ApplicationDbContext context)
        {
            _categoryRepository = categoryRepository;
            _productRepository = productRepository;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Tải danh mục kèm theo sản phẩm
                var categories = await _context.Categories
                    .Include(c => c.Products)
                    .ToListAsync();

                return View(categories);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi tải danh sách danh mục: " + ex.Message;
                return View(new List<Category>());
            }
        }

        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(Category category)
        {
            try
            {
                // Loại bỏ Products khỏi ModelState validation
                ModelState.Remove("Products");

                if (ModelState.IsValid)
                {
                    // Kiểm tra trùng lặp tên danh mục
                    var existingCategory = await _context.Categories
                        .FirstOrDefaultAsync(c => c.Name.ToLower() == category.Name.ToLower());

                    if (existingCategory != null)
                    {
                        ModelState.AddModelError("Name", "Tên danh mục đã tồn tại!");
                        return View(category);
                    }

                    await _categoryRepository.AddAsync(category);
                    TempData["SuccessMessage"] = "Thêm danh mục thành công!";
                    return RedirectToAction(nameof(Index));
                }
                return View(category);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi thêm danh mục: " + ex.Message;
                return View(category);
            }
        }

        public async Task<IActionResult> Update(int id)
        {
            try
            {
                var category = await _context.Categories
                    .Include(c => c.Products)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (category == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy danh mục!";
                    return RedirectToAction(nameof(Index));
                }
                return View(category);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi tải form cập nhật: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, Category category)
        {
            if (id != category.Id)
            {
                TempData["ErrorMessage"] = "ID danh mục không hợp lệ!";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                ModelState.Remove("Products");

                if (ModelState.IsValid)
                {
                    // Kiểm tra trùng lặp tên danh mục (loại trừ danh mục hiện tại)
                    var existingCategory = await _context.Categories
                        .FirstOrDefaultAsync(c => c.Name.ToLower() == category.Name.ToLower() && c.Id != id);

                    if (existingCategory != null)
                    {
                        ModelState.AddModelError("Name", "Tên danh mục đã tồn tại!");
                        var categoryWithProducts = await _context.Categories
                            .Include(c => c.Products)
                            .FirstOrDefaultAsync(c => c.Id == id);
                        return View(categoryWithProducts);
                    }

                    await _categoryRepository.UpdateAsync(category);
                    TempData["SuccessMessage"] = "Cập nhật danh mục thành công!";
                    return RedirectToAction(nameof(Index));
                }

                // Tải lại danh mục với sản phẩm nếu validation thất bại
                var cat = await _context.Categories
                    .Include(c => c.Products)
                    .FirstOrDefaultAsync(c => c.Id == id);
                return View(cat);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi cập nhật danh mục: " + ex.Message;
                var cat = await _context.Categories
                    .Include(c => c.Products)
                    .FirstOrDefaultAsync(c => c.Id == id);
                return View(cat);
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var category = await _context.Categories
                    .Include(c => c.Products)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (category == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy danh mục!";
                    return RedirectToAction(nameof(Index));
                }

                return View(category);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi tải trang xóa danh mục: " + ex.Message;
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
                var category = await _context.Categories
                    .Include(c => c.Products)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (category == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy danh mục!";
                    return RedirectToAction(nameof(Index));
                }

                // Kiểm tra xem danh mục có sản phẩm không
                if (category.Products != null && category.Products.Any())
                {
                    TempData["ErrorMessage"] = $"Không thể xóa danh mục này vì có {category.Products.Count} sản phẩm đang thuộc về danh mục.";
                    return RedirectToAction(nameof(Index));
                }

                await _categoryRepository.DeleteAsync(id);
                TempData["SuccessMessage"] = "Xóa danh mục thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi xóa danh mục: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}