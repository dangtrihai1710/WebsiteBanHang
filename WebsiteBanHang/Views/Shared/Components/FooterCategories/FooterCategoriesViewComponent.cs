using Microsoft.AspNetCore.Mvc;
using WebsiteBanHang.Repositories;

namespace WebsiteBanHang.Views.Shared.Components.FooterCategories
{
    public class FooterCategoriesViewComponent : ViewComponent
    {
        private readonly ICategoryRepository _categoryRepository;

        public FooterCategoriesViewComponent(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return View(categories.Take(6)); // Chỉ lấy 6 danh mục đầu tiên cho footer
        }
    }
}