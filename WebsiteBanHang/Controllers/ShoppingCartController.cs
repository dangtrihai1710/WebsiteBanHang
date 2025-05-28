// Controllers/ShoppingCartController.cs
using Microsoft.AspNetCore.Mvc;
using WebsiteBanHang.Models;
using WebsiteBanHang.Repositories;
using WebsiteBanHang.Extensions;

namespace WebsiteBanHang.Controllers
{
    public class ShoppingCartController : Controller
    {
        private readonly IProductRepository _productRepository;
        private const string CartSessionKey = "Cart";

        public ShoppingCartController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        // Hiển thị giỏ hàng
        public IActionResult Index()
        {
            var cart = GetCartFromSession();
            return View(cart);
        }

        // Thêm sản phẩm vào giỏ hàng
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            try
            {
                var product = await _productRepository.GetByIdAsync(productId);
                if (product == null)
                {
                    return Json(new { success = false, message = "Sản phẩm không tồn tại!" });
                }

                var cartItem = new CartItem
                {
                    ProductId = productId,
                    Name = product.Name,
                    Price = product.Price,
                    Quantity = quantity,
                    ImageUrl = product.ImageUrl
                };

                var cart = GetCartFromSession();
                cart.AddItem(cartItem);
                SaveCartToSession(cart);

                return Json(new
                {
                    success = true,
                    message = "Đã thêm sản phẩm vào giỏ hàng!",
                    cartCount = cart.GetTotalQuantity(),
                    cartTotal = cart.GetTotalPrice()
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        // Cập nhật số lượng sản phẩm
        [HttpPost]
        public IActionResult UpdateQuantity(int productId, int quantity)
        {
            try
            {
                var cart = GetCartFromSession();
                cart.UpdateQuantity(productId, quantity);
                SaveCartToSession(cart);

                return Json(new
                {
                    success = true,
                    message = "Cập nhật thành công!",
                    cartCount = cart.GetTotalQuantity(),
                    cartTotal = cart.GetTotalPrice()
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        // Xóa sản phẩm khỏi giỏ hàng
        public IActionResult RemoveFromCart(int productId)
        {
            var cart = GetCartFromSession();
            cart.RemoveItem(productId);
            SaveCartToSession(cart);

            TempData["SuccessMessage"] = "Đã xóa sản phẩm khỏi giỏ hàng!";
            return RedirectToAction("Index");
        }

        // Xóa toàn bộ giỏ hàng
        public IActionResult ClearCart()
        {
            HttpContext.Session.Remove(CartSessionKey);
            TempData["SuccessMessage"] = "Đã xóa toàn bộ giỏ hàng!";
            return RedirectToAction("Index");
        }

        // Lấy số lượng sản phẩm trong giỏ hàng (API)
        [HttpGet]
        public IActionResult GetCartCount()
        {
            var cart = GetCartFromSession();
            return Json(new { count = cart.GetTotalQuantity() });
        }

        // Trang thanh toán
        public IActionResult Checkout()
        {
            var cart = GetCartFromSession();
            if (!cart.Items.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng của bạn đang trống!";
                return RedirectToAction("Index");
            }

            return View(cart);
        }

        // Phương thức hỗ trợ: Lấy giỏ hàng từ Session
        private ShoppingCart GetCartFromSession()
        {
            return HttpContext.Session.GetObjectFromJson<ShoppingCart>(CartSessionKey)
                   ?? new ShoppingCart();
        }

        // Phương thức hỗ trợ: Lưu giỏ hàng vào Session
        private void SaveCartToSession(ShoppingCart cart)
        {
            HttpContext.Session.SetObjectAsJson(CartSessionKey, cart);
        }
    }
}