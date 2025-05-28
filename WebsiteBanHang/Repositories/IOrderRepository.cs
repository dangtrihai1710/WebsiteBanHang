// Repositories/IOrderRepository.cs
using WebsiteBanHang.Models;

namespace WebsiteBanHang.Repositories
{
    public interface IOrderRepository
    {
        Task<IEnumerable<Order>> GetAllAsync();
        Task<IEnumerable<Order>> GetByUserIdAsync(string userId);
        Task<Order> GetByIdAsync(int id);
        Task<Order> GetByIdWithDetailsAsync(int id);
        Task AddAsync(Order order);
        Task UpdateAsync(Order order);
        Task DeleteAsync(int id);
        Task<IEnumerable<Order>> GetRecentOrdersAsync(int count = 10);
        Task<decimal> GetTotalRevenueAsync();
        Task<int> GetTotalOrdersCountAsync();
    }
}

