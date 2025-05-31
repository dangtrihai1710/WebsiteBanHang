using WebsiteBanHang.Models;

namespace WebsiteBanHang.Repositories
{
    public interface IProductImageRepository
    {
        Task<IEnumerable<ProductImage>> GetByProductIdAsync(int productId);
        Task<ProductImage?> GetMainImageByProductIdAsync(int productId);
        Task AddAsync(ProductImage productImage);
        Task AddRangeAsync(IEnumerable<ProductImage> productImages);
        Task UpdateAsync(ProductImage productImage);
        Task DeleteAsync(int id);
        Task DeleteByProductIdAsync(int productId);
        Task SetMainImageAsync(int productId, int imageId);
    }
}