using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebsiteBanHang.Models;
using WebsiteBanHang.Repositories;

namespace WebsiteBanHang.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IProductRepository _productRepository;

        public HomeController(ILogger<HomeController> logger, IProductRepository productRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        }

        public IActionResult Index()
        {
            try
            {
                var products = _productRepository.GetAll();

                // Nếu không có sản phẩm, trả về danh sách rỗng thay vì null
                if (products == null)
                {
                    products = new List<Product>();
                }

                return View(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting products for home page");
                return View(new List<Product>()); // Trả về danh sách rỗng khi có lỗi
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}