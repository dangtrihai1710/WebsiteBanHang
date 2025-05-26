using Microsoft.EntityFrameworkCore;
using WebsiteBanHang.Models;

namespace WebsiteBanHang.Repositories
{
    public class EFCategoryRepository : ICategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public EFCategoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            try
            {
                return await _context.Categories
                    .Include(c => c.Products)
                    .OrderByDescending(c => c.Id)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy danh sách danh mục: {ex.Message}", ex);
            }
        }

        public async Task<Category> GetByIdAsync(int id)
        {
            try
            {
                var category = await _context.Categories
                    .Include(c => c.Products)
                    .FirstOrDefaultAsync(c => c.Id == id);

                return category;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy danh mục ID {id}: {ex.Message}", ex);
            }
        }

        public async Task AddAsync(Category category)
        {
            try
            {
                if (category == null)
                    throw new ArgumentNullException(nameof(category));

                // Validate required fields
                if (string.IsNullOrWhiteSpace(category.Name))
                    throw new ArgumentException("Tên danh mục không được để trống");

                // Check if category name already exists
                var existingCategory = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Name.ToLower() == category.Name.ToLower());

                if (existingCategory != null)
                    throw new ArgumentException("Tên danh mục đã tồn tại");

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi thêm danh mục: {ex.Message}", ex);
            }
        }

        public async Task UpdateAsync(Category category)
        {
            try
            {
                if (category == null)
                    throw new ArgumentNullException(nameof(category));

                // Validate required fields
                if (string.IsNullOrWhiteSpace(category.Name))
                    throw new ArgumentException("Tên danh mục không được để trống");

                // Check if category exists
                var existingCategory = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Id == category.Id);

                if (existingCategory == null)
                    throw new ArgumentException("Danh mục không tồn tại");

                // Check if name already exists (exclude current category)
                var duplicateCategory = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Name.ToLower() == category.Name.ToLower() && c.Id != category.Id);

                if (duplicateCategory != null)
                    throw new ArgumentException("Tên danh mục đã tồn tại");

                // Update using Entry to avoid tracking issues
                _context.Entry(existingCategory).CurrentValues.SetValues(category);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi cập nhật danh mục: {ex.Message}", ex);
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                var category = await _context.Categories
                    .Include(c => c.Products)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (category == null)
                    throw new ArgumentException("Danh mục không tồn tại");

                // Check if category has products
                if (category.Products != null && category.Products.Any())
                    throw new ArgumentException($"Không thể xóa danh mục này vì có {category.Products.Count} sản phẩm đang thuộc về danh mục");

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi xóa danh mục: {ex.Message}", ex);
            }
        }
    }
}