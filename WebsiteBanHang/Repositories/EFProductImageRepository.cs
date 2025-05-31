using Microsoft.EntityFrameworkCore;
using WebsiteBanHang.Models;

namespace WebsiteBanHang.Repositories
{
    public class EFProductImageRepository : IProductImageRepository
    {
        private readonly ApplicationDbContext _context;

        public EFProductImageRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProductImage>> GetByProductIdAsync(int productId)
        {
            return await _context.ProductImages
                .Where(pi => pi.ProductId == productId)
                .OrderBy(pi => pi.DisplayOrder)
                .ThenBy(pi => pi.Id)
                .ToListAsync();
        }

        public async Task<ProductImage?> GetMainImageByProductIdAsync(int productId)
        {
            // Tìm ảnh chính trước
            var mainImage = await _context.ProductImages
                .Where(pi => pi.ProductId == productId && pi.IsMain)
                .FirstOrDefaultAsync();

            // Nếu không có ảnh chính, lấy ảnh đầu tiên
            if (mainImage == null)
            {
                mainImage = await _context.ProductImages
                    .Where(pi => pi.ProductId == productId)
                    .OrderBy(pi => pi.DisplayOrder)
                    .ThenBy(pi => pi.Id)
                    .FirstOrDefaultAsync();
            }

            return mainImage;
        }

        public async Task AddAsync(ProductImage productImage)
        {
            _context.ProductImages.Add(productImage);
            await _context.SaveChangesAsync();
        }

        public async Task AddRangeAsync(IEnumerable<ProductImage> productImages)
        {
            _context.ProductImages.AddRange(productImages);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ProductImage productImage)
        {
            _context.ProductImages.Update(productImage);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var productImage = await _context.ProductImages.FindAsync(id);
            if (productImage != null)
            {
                _context.ProductImages.Remove(productImage);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteByProductIdAsync(int productId)
        {
            var images = await _context.ProductImages
                .Where(pi => pi.ProductId == productId)
                .ToListAsync();

            _context.ProductImages.RemoveRange(images);
            await _context.SaveChangesAsync();
        }

        public async Task SetMainImageAsync(int productId, int imageId)
        {
            // Bỏ chọn tất cả ảnh chính hiện tại
            var currentMainImages = await _context.ProductImages
                .Where(pi => pi.ProductId == productId && pi.IsMain)
                .ToListAsync();

            foreach (var img in currentMainImages)
            {
                img.IsMain = false;
            }

            // Đặt ảnh mới làm ảnh chính
            var newMainImage = await _context.ProductImages.FindAsync(imageId);
            if (newMainImage != null && newMainImage.ProductId == productId)
            {
                newMainImage.IsMain = true;
            }

            await _context.SaveChangesAsync();
        }
    }
}