using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using WebsiteBanHang.Models;
using WebsiteBanHang.Repositories;

namespace WebsiteBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IProductImageRepository _productImageRepository;

        public ProductController(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IProductImageRepository productImageRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _productImageRepository = productImageRepository;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var products = await _productRepository.GetAllAsync();

                // Lấy ảnh chính cho mỗi sản phẩm
                foreach (var product in products)
                {
                    var mainImage = await _productImageRepository.GetMainImageByProductIdAsync(product.Id);
                    product.ImageUrl = mainImage?.Url;
                }

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
        public async Task<IActionResult> Add(Product product, List<IFormFile> images)
        {
            try
            {
                ModelState.Remove("ImageUrl");
                ModelState.Remove("Category");
                ModelState.Remove("Images");

                if (ModelState.IsValid)
                {
                    // Thêm sản phẩm trước
                    await _productRepository.AddAsync(product);

                    // Xử lý upload nhiều ảnh
                    if (images != null && images.Count > 0)
                    {
                        var productImages = new List<ProductImage>();

                        for (int i = 0; i < images.Count; i++)
                        {
                            var image = images[i];
                            if (image != null && image.Length > 0)
                            {
                                var imageUrl = await SaveImage(image);
                                var productImage = new ProductImage
                                {
                                    ProductId = product.Id,
                                    Url = imageUrl,
                                    IsMain = i == 0, // Ảnh đầu tiên là ảnh chính
                                    DisplayOrder = i
                                };
                                productImages.Add(productImage);
                            }
                        }

                        if (productImages.Any())
                        {
                            await _productImageRepository.AddRangeAsync(productImages);

                            // Cập nhật ImageUrl cho product với ảnh chính
                            var mainImage = productImages.FirstOrDefault(pi => pi.IsMain);
                            if (mainImage != null)
                            {
                                product.ImageUrl = mainImage.Url;
                                await _productRepository.UpdateAsync(product);
                            }
                        }
                    }

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

                // Lấy tất cả ảnh của sản phẩm
                var images = await _productImageRepository.GetByProductIdAsync(id);
                product.Images = images.ToList();

                // Đặt ảnh chính
                var mainImage = await _productImageRepository.GetMainImageByProductIdAsync(id);
                product.ImageUrl = mainImage?.Url;

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

                // Lấy tất cả ảnh của sản phẩm
                var images = await _productImageRepository.GetByProductIdAsync(id);
                product.Images = images.ToList();

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
        public async Task<IActionResult> Update(int id, Product product, List<IFormFile> newImages, string? removeImageIds)
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

                    // Cập nhật thông tin sản phẩm
                    existingProduct.Name = product.Name;
                    existingProduct.Price = product.Price;
                    existingProduct.Description = product.Description;
                    existingProduct.CategoryId = product.CategoryId;

                    // Xử lý xóa ảnh cũ
                    if (!string.IsNullOrEmpty(removeImageIds))
                    {
                        var idsToRemove = removeImageIds.Split(',')
                            .Where(x => int.TryParse(x, out _))
                            .Select(int.Parse);

                        foreach (var imageId in idsToRemove)
                        {
                            await _productImageRepository.DeleteAsync(imageId);
                        }
                    }

                    // Xử lý thêm ảnh mới
                    if (newImages != null && newImages.Count > 0)
                    {
                        var existingImages = await _productImageRepository.GetByProductIdAsync(id);
                        var maxOrder = existingImages.Any() ? existingImages.Max(i => i.DisplayOrder) : -1;

                        var productImages = new List<ProductImage>();

                        for (int i = 0; i < newImages.Count; i++)
                        {
                            var image = newImages[i];
                            if (image != null && image.Length > 0)
                            {
                                var imageUrl = await SaveImage(image);
                                var productImage = new ProductImage
                                {
                                    ProductId = product.Id,
                                    Url = imageUrl,
                                    IsMain = !existingImages.Any() && i == 0, // Chỉ đặt làm ảnh chính nếu chưa có ảnh nào
                                    DisplayOrder = maxOrder + i + 1
                                };
                                productImages.Add(productImage);
                            }
                        }

                        if (productImages.Any())
                        {
                            await _productImageRepository.AddRangeAsync(productImages);
                        }
                    }

                    // Cập nhật ImageUrl cho product với ảnh chính
                    var mainImage = await _productImageRepository.GetMainImageByProductIdAsync(id);
                    existingProduct.ImageUrl = mainImage?.Url;

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

                // Lấy ảnh chính để hiển thị
                var mainImage = await _productImageRepository.GetMainImageByProductIdAsync(id);
                product.ImageUrl = mainImage?.Url;

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
                // Xóa tất cả ảnh của sản phẩm trước
                await _productImageRepository.DeleteByProductIdAsync(id);

                // Sau đó xóa sản phẩm
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

        // API để set ảnh chính
        [HttpPost]
        public async Task<IActionResult> SetMainImage(int productId, int imageId)
        {
            try
            {
                await _productImageRepository.SetMainImageAsync(productId, imageId);

                // Cập nhật ImageUrl cho product
                var mainImage = await _productImageRepository.GetMainImageByProductIdAsync(productId);
                var product = await _productRepository.GetByIdAsync(productId);
                if (product != null && mainImage != null)
                {
                    product.ImageUrl = mainImage.Url;
                    await _productRepository.UpdateAsync(product);
                }

                return Json(new { success = true, message = "Đã đặt làm ảnh chính!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
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