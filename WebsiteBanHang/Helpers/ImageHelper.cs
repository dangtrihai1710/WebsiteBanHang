// Helpers/ImageHelper.cs
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using WebsiteBanHang.Helpers;
using WebsiteBanHang.Repositories;

namespace WebsiteBanHang.Helpers
{
    public static class ImageHelper
    {
        /// <summary>
        /// Kiểm tra ảnh có tồn tại không
        /// </summary>
        public static bool ImageExists(string imageUrl, IWebHostEnvironment environment)
        {
            if (string.IsNullOrEmpty(imageUrl))
                return false;

            try
            {
                // Chuyển URL thành đường dẫn file
                var imagePath = imageUrl.StartsWith("/") ? imageUrl.Substring(1) : imageUrl;
                var fullPath = Path.Combine(environment.WebRootPath, imagePath.Replace("/", Path.DirectorySeparatorChar.ToString()));

                return File.Exists(fullPath);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Lấy URL ảnh mặc định nếu ảnh không tồn tại
        /// </summary>
        public static string GetValidImageUrl(string imageUrl, IWebHostEnvironment environment)
        {
            if (ImageExists(imageUrl, environment))
                return imageUrl;

            // Trả về ảnh placeholder
            return "/images/placeholder.jpg";
        }

        /// <summary>
        /// Làm sạch tất cả ảnh không tồn tại trong database
        /// </summary>
        public static async Task<int> CleanupMissingImages(
            IProductRepository productRepo,
            IProductImageRepository imageRepo,
            IWebHostEnvironment environment)
        {
            int cleanedCount = 0;
            var products = await productRepo.GetAllAsync();

            foreach (var product in products)
            {
                // Kiểm tra ImageUrl chính
                if (!string.IsNullOrEmpty(product.ImageUrl) && !ImageExists(product.ImageUrl, environment))
                {
                    // Tìm ảnh thay thế từ ProductImages
                    var validImage = await imageRepo.GetMainImageByProductIdAsync(product.Id);
                    if (validImage != null && ImageExists(validImage.Url, environment))
                    {
                        product.ImageUrl = validImage.Url;
                        await productRepo.UpdateAsync(product);
                        cleanedCount++;
                    }
                    else
                    {
                        product.ImageUrl = "/images/placeholder.jpg";
                        await productRepo.UpdateAsync(product);
                        cleanedCount++;
                    }
                }

                // Kiểm tra và xóa các ProductImage không tồn tại
                if (product.Images != null)
                {
                    foreach (var image in product.Images.ToList())
                    {
                        if (!ImageExists(image.Url, environment))
                        {
                            await imageRepo.DeleteAsync(image.Id);
                            cleanedCount++;
                        }
                    }
                }
            }

            return cleanedCount;
        }
    }
}

