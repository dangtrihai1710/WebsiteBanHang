// Repositories/EFOrderRepository.cs
using Microsoft.EntityFrameworkCore;
using WebsiteBanHang.Models;

namespace WebsiteBanHang.Repositories
{
    public class EFOrderRepository : IOrderRepository
    {
        private readonly ApplicationDbContext _context;

        public EFOrderRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Order>> GetAllAsync()
        {
            try
            {
                return await _context.Orders
                    .Include(o => o.ApplicationUser)
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Product)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy danh sách đơn hàng: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<Order>> GetByUserIdAsync(string userId)
        {
            try
            {
                return await _context.Orders
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Product)
                    .Where(o => o.UserId == userId)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy đơn hàng của user {userId}: {ex.Message}", ex);
            }
        }

        public async Task<Order> GetByIdAsync(int id)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.ApplicationUser)
                    .FirstOrDefaultAsync(o => o.Id == id);

                return order;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy đơn hàng ID {id}: {ex.Message}", ex);
            }
        }

        public async Task<Order> GetByIdWithDetailsAsync(int id)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.ApplicationUser)
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Product)
                            .ThenInclude(p => p.Category)
                    .FirstOrDefaultAsync(o => o.Id == id);

                return order;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy chi tiết đơn hàng ID {id}: {ex.Message}", ex);
            }
        }

        public async Task AddAsync(Order order)
        {
            try
            {
                if (order == null)
                    throw new ArgumentNullException(nameof(order));

                // Validate required fields
                if (string.IsNullOrWhiteSpace(order.UserId))
                    throw new ArgumentException("UserId không được để trống");

                if (string.IsNullOrWhiteSpace(order.ShippingAddress))
                    throw new ArgumentException("Địa chỉ giao hàng không được để trống");

                if (order.TotalPrice <= 0)
                    throw new ArgumentException("Tổng tiền phải lớn hơn 0");

                if (order.OrderDetails == null || !order.OrderDetails.Any())
                    throw new ArgumentException("Đơn hàng phải có ít nhất một sản phẩm");

                // Set order date if not set
                if (order.OrderDate == default)
                    order.OrderDate = DateTime.UtcNow;

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi tạo đơn hàng: {ex.Message}", ex);
            }
        }

        public async Task UpdateAsync(Order order)
        {
            try
            {
                if (order == null)
                    throw new ArgumentNullException(nameof(order));

                var existingOrder = await _context.Orders
                    .FirstOrDefaultAsync(o => o.Id == order.Id);

                if (existingOrder == null)
                    throw new ArgumentException("Đơn hàng không tồn tại");

                // Update properties

                existingOrder.ShippingAddress = order.ShippingAddress;
                existingOrder.Notes = order.Notes;

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi cập nhật đơn hàng: {ex.Message}", ex);
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderDetails)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order == null)
                    throw new ArgumentException("Đơn hàng không tồn tại");

                // Remove order details first
                _context.OrderDetails.RemoveRange(order.OrderDetails);

                // Remove order
                _context.Orders.Remove(order);

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi xóa đơn hàng: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<Order>> GetRecentOrdersAsync(int count = 10)
        {
            try
            {
                return await _context.Orders
                    .Include(o => o.ApplicationUser)
                    .OrderByDescending(o => o.OrderDate)
                    .Take(count)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy đơn hàng gần đây: {ex.Message}", ex);
            }
        }

        public async Task<decimal> GetTotalRevenueAsync()
        {
            try
            {
                return await _context.Orders
                    .SumAsync(o => o.TotalPrice);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi tính tổng doanh thu: {ex.Message}", ex);
            }
        }

        public async Task<int> GetTotalOrdersCountAsync()
        {
            try
            {
                return await _context.Orders.CountAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi đếm tổng số đơn hàng: {ex.Message}", ex);
            }
        }
    }
}