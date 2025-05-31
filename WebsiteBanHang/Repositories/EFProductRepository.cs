using Microsoft.EntityFrameworkCore;
using WebsiteBanHang.Models;

namespace WebsiteBanHang.Repositories
{
    public class EFProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;

        public EFProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            try
            {
                return await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Images) // ✅ Include Images
                    .OrderByDescending(p => p.Id)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy danh sách sản phẩm: {ex.Message}", ex);
            }
        }

        public async Task<Product> GetByIdAsync(int id)
        {
            try
            {
                var product = await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Images) // ✅ Include Images
                    .FirstOrDefaultAsync(p => p.Id == id);

                return product;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy sản phẩm ID {id}: {ex.Message}", ex);
            }
        }

        public async Task AddAsync(Product product)
        {
            try
            {
                if (product == null)
                    throw new ArgumentNullException(nameof(product));

                // Validate required fields
                if (string.IsNullOrWhiteSpace(product.Name))
                    throw new ArgumentException("Tên sản phẩm không được để trống");

                if (product.Price <= 0)
                    throw new ArgumentException("Giá sản phẩm phải lớn hơn 0");

                if (product.CategoryId <= 0)
                    throw new ArgumentException("Phải chọn danh mục cho sản phẩm");

                // Check if category exists
                var categoryExists = await _context.Categories
                    .AnyAsync(c => c.Id == product.CategoryId);

                if (!categoryExists)
                    throw new ArgumentException("Danh mục không tồn tại");

                _context.Products.Add(product);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi thêm sản phẩm: {ex.Message}", ex);
            }
        }

        public async Task UpdateAsync(Product product)
        {
            try
            {
                if (product == null)
                    throw new ArgumentNullException(nameof(product));

                // Validate required fields
                if (string.IsNullOrWhiteSpace(product.Name))
                    throw new ArgumentException("Tên sản phẩm không được để trống");

                if (product.Price <= 0)
                    throw new ArgumentException("Giá sản phẩm phải lớn hơn 0");

                if (product.CategoryId <= 0)
                    throw new ArgumentException("Phải chọn danh mục cho sản phẩm");

                // Check if category exists
                var categoryExists = await _context.Categories
                    .AnyAsync(c => c.Id == product.CategoryId);

                if (!categoryExists)
                    throw new ArgumentException("Danh mục không tồn tại");

                // Check if product exists
                var existingProduct = await _context.Products
                    .FirstOrDefaultAsync(p => p.Id == product.Id);

                if (existingProduct == null)
                    throw new ArgumentException("Sản phẩm không tồn tại");

                // Update using Entry to avoid tracking issues
                _context.Entry(existingProduct).CurrentValues.SetValues(product);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi cập nhật sản phẩm: {ex.Message}", ex);
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                var product = await _context.Products
                    .Include(p => p.Images) // ✅ Include Images để xóa cùng
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (product == null)
                    throw new ArgumentException("Sản phẩm không tồn tại");

                // Xóa tất cả ảnh của sản phẩm trước
                if (product.Images != null && product.Images.Any())
                {
                    _context.ProductImages.RemoveRange(product.Images);
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi xóa sản phẩm: {ex.Message}", ex);
            }
        }
    }
}