// Areas/Admin/Controllers/ProductController.cs - CODE HOÀN CHỈNH ĐÃ SỬA
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
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ProductController> _logger;

        public ProductController(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IProductImageRepository productImageRepository,
            IWebHostEnvironment environment,
            ILogger<ProductController> logger)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _productImageRepository = productImageRepository;
            _environment = environment;
            _logger = logger;
        }

        // ✅ SỬA: Index với logic đồng bộ ảnh chính
        public async Task<IActionResult> Index()
        {
            try
            {
                var products = await _productRepository.GetAllAsync();

                // ✅ FIX: Đồng bộ ảnh chính cho mỗi sản phẩm
                foreach (var product in products)
                {
                    // Lấy ảnh chính từ ProductImages
                    var mainImage = await _productImageRepository.GetMainImageByProductIdAsync(product.Id);

                    if (mainImage != null)
                    {
                        // Cập nhật ImageUrl nếu khác
                        if (product.ImageUrl != mainImage.Url)
                        {
                            product.ImageUrl = mainImage.Url;
                            await _productRepository.UpdateAsync(product);
                        }
                    }
                    else if (product.Images != null && product.Images.Any())
                    {
                        // Nếu không có ảnh chính, lấy ảnh đầu tiên và đặt làm chính
                        var firstImage = product.Images.OrderBy(i => i.DisplayOrder).First();
                        await _productImageRepository.SetMainImageAsync(product.Id, firstImage.Id);
                        product.ImageUrl = firstImage.Url;
                        await _productRepository.UpdateAsync(product);
                    }
                }

                return View(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading products list");
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

        // ✅ THÊM: API để set ảnh chính
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

        // ✅ THÊM: API đồng bộ tất cả ảnh
        [HttpPost]
        public async Task<IActionResult> SyncAllImages()
        {
            try
            {
                var products = await _productRepository.GetAllAsync();
                int syncedCount = 0;
                var errors = new List<string>();

                foreach (var product in products)
                {
                    try
                    {
                        var mainImage = await _productImageRepository.GetMainImageByProductIdAsync(product.Id);

                        if (mainImage != null && product.ImageUrl != mainImage.Url)
                        {
                            product.ImageUrl = mainImage.Url;
                            await _productRepository.UpdateAsync(product);
                            syncedCount++;
                        }
                        else if (mainImage == null && product.Images != null && product.Images.Any())
                        {
                            // Nếu không có ảnh chính, đặt ảnh đầu tiên làm ảnh chính
                            var firstImage = product.Images.OrderBy(i => i.DisplayOrder).First();
                            await _productImageRepository.SetMainImageAsync(product.Id, firstImage.Id);

                            product.ImageUrl = firstImage.Url;
                            await _productRepository.UpdateAsync(product);
                            syncedCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Sản phẩm {product.Name}: {ex.Message}");
                    }
                }

                var message = $"Đã đồng bộ {syncedCount} sản phẩm thành công";
                if (errors.Any())
                {
                    message += $". Có {errors.Count} lỗi xảy ra.";
                }

                _logger.LogInformation($"Synced {syncedCount} product images. Errors: {errors.Count}");

                return Json(new
                {
                    success = true,
                    message = message,
                    syncedCount = syncedCount,
                    errorCount = errors.Count,
                    errors = errors
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing all images");
                return Json(new
                {
                    success = false,
                    message = "Lỗi khi đồng bộ ảnh: " + ex.Message
                });
            }
        }

        // ✅ THÊM: API đồng bộ ảnh đơn lẻ
        [HttpPost]
        public async Task<IActionResult> SyncSingleProductImage(int productId)
        {
            try
            {
                var product = await _productRepository.GetByIdAsync(productId);
                if (product == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy sản phẩm" });
                }

                var mainImage = await _productImageRepository.GetMainImageByProductIdAsync(productId);

                if (mainImage != null)
                {
                    product.ImageUrl = mainImage.Url;
                    await _productRepository.UpdateAsync(product);

                    return Json(new
                    {
                        success = true,
                        message = "Đã đồng bộ ảnh thành công",
                        newImageUrl = mainImage.Url
                    });
                }
                else
                {
                    // Nếu không có ảnh chính, kiểm tra có ảnh nào khác không
                    var images = await _productImageRepository.GetByProductIdAsync(productId);
                    if (images.Any())
                    {
                        var firstImage = images.OrderBy(i => i.DisplayOrder).First();
                        await _productImageRepository.SetMainImageAsync(productId, firstImage.Id);

                        product.ImageUrl = firstImage.Url;
                        await _productRepository.UpdateAsync(product);

                        return Json(new
                        {
                            success = true,
                            message = "Đã đặt ảnh đầu tiên làm ảnh chính",
                            newImageUrl = firstImage.Url
                        });
                    }
                    else
                    {
                        return Json(new
                        {
                            success = false,
                            message = "Sản phẩm không có ảnh nào"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error syncing single product image for product {productId}");
                return Json(new
                {
                    success = false,
                    message = "Lỗi: " + ex.Message
                });
            }
        }

        // ✅ THÊM: API làm sạch ảnh không tồn tại
        [HttpPost]
        public async Task<IActionResult> CleanupMissingImages()
        {
            try
            {
                var products = await _productRepository.GetAllAsync();
                int cleanedCount = 0;

                foreach (var product in products)
                {
                    // Kiểm tra ImageUrl chính
                    if (!string.IsNullOrEmpty(product.ImageUrl) && !ImageExists(product.ImageUrl))
                    {
                        // Tìm ảnh thay thế từ ProductImages
                        var validImage = await _productImageRepository.GetMainImageByProductIdAsync(product.Id);
                        if (validImage != null && ImageExists(validImage.Url))
                        {
                            product.ImageUrl = validImage.Url;
                            await _productRepository.UpdateAsync(product);
                            cleanedCount++;
                        }
                        else
                        {
                            product.ImageUrl = null; // Set null thay vì placeholder
                            await _productRepository.UpdateAsync(product);
                            cleanedCount++;
                        }
                    }

                    // Kiểm tra và xóa các ProductImage không tồn tại
                    if (product.Images != null)
                    {
                        foreach (var image in product.Images.ToList())
                        {
                            if (!ImageExists(image.Url))
                            {
                                await _productImageRepository.DeleteAsync(image.Id);
                                cleanedCount++;
                            }
                        }
                    }
                }

                return Json(new
                {
                    success = true,
                    message = $"Đã làm sạch {cleanedCount} ảnh không tồn tại",
                    cleanedCount = cleanedCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up missing images");
                return Json(new
                {
                    success = false,
                    message = "Lỗi: " + ex.Message
                });
            }
        }

        // ✅ THÊM: Helper method kiểm tra ảnh tồn tại
        private bool ImageExists(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
                return false;

            try
            {
                // Chuyển URL thành đường dẫn file
                var imagePath = imageUrl.StartsWith("/") ? imageUrl.Substring(1) : imageUrl;
                var fullPath = Path.Combine(_environment.WebRootPath, imagePath.Replace("/", Path.DirectorySeparatorChar.ToString()));

                return System.IO.File.Exists(fullPath);
            }
            catch
            {
                return false;
            }
        }

        // ✅ THÊM: Helper method lưu ảnh với validation
        private async Task<string> SaveImage(IFormFile image)
        {
            try
            {
                // Kiểm tra file hợp lệ
                if (image == null || image.Length == 0)
                    throw new ArgumentException("File ảnh không hợp lệ");

                // Kiểm tra định dạng file
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var fileExtension = Path.GetExtension(image.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(fileExtension))
                    throw new ArgumentException("Chỉ chấp nhận file ảnh (.jpg, .jpeg, .png, .gif, .webp)");

                // Kiểm tra kích thước file (max 5MB)
                if (image.Length > 5 * 1024 * 1024)
                    throw new ArgumentException("File ảnh không được vượt quá 5MB");

                // Tạo thư mục nếu chưa có
                var imagesFolder = Path.Combine(_environment.WebRootPath, "images");
                if (!Directory.Exists(imagesFolder))
                {
                    Directory.CreateDirectory(imagesFolder);
                }

                // Tạo tên file duy nhất
                var fileName = Guid.NewGuid().ToString() + fileExtension;
                var filePath = Path.Combine(imagesFolder, fileName);

                // Lưu file
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(fileStream);
                }

                _logger.LogInformation($"Saved image: {fileName}");
                return "/images/" + fileName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving image");
                throw new Exception("Lỗi khi lưu hình ảnh: " + ex.Message);
            }
        }
    }
}