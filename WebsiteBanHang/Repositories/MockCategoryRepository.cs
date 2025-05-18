using System.Collections.Generic;
using System.Threading.Tasks;
using WebsiteBanHang.Models;

namespace WebsiteBanHang.Repositories
{
    public class MockCategoryRepository : ICategoryRepository
    {
        private List<Category> _categoryList;

        public MockCategoryRepository()
        {
            _categoryList = new List<Category>
            {
                new Category { Id = 1, Name = "Laptop" },
                new Category { Id = 2, Name = "Desktop" },
                // Thêm các category khác
            };
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await Task.FromResult(_categoryList);
        }

        public async Task<Category> GetByIdAsync(int id)
        {
            return await Task.FromResult(_categoryList.Find(c => c.Id == id));
        }

        public async Task AddAsync(Category category)
        {
            _categoryList.Add(category);
            await Task.CompletedTask;
        }

        public async Task UpdateAsync(Category category)
        {
            var index = _categoryList.FindIndex(c => c.Id == category.Id);
            if (index != -1)
            {
                _categoryList[index] = category;
            }
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(int id)
        {
            var category = _categoryList.Find(c => c.Id == id);
            if (category != null)
            {
                _categoryList.Remove(category);
            }
            await Task.CompletedTask;
        }
    }
}