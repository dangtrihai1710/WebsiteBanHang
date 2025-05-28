using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using WebsiteBanHang.Models;
using WebsiteBanHang.Repositories;
using WebsiteBanHang.Extensions;

namespace WebsiteBanHang.Controllers
{
    public class ShoppingCartController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOrderRepository _orderRepository;
        private const string CartSessionKey = "Cart";

        public ShoppingCartController(
            IProductRepository productRepository,
            UserManager<ApplicationUser> userManager,
            IOrderRepository orderRepository)
        {
            _productRepository = productRepository;
            _userManager = userManager;
            _orderRepository = orderRepository;
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
            var cartKey = GetCartSessionKey();
            HttpContext.Session.Remove(cartKey);
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

        // Phương thức hỗ trợ: Tạo key giỏ hàng duy nhất cho từng user
        private string GetCartSessionKey()
        {
            if (User.Identity.IsAuthenticated)
            {
                // Nếu đã đăng nhập, sử dụng UserId
                var userId = _userManager.GetUserId(User);
                return $"{CartSessionKey}_{userId}";
            }
            else
            {
                // Nếu chưa đăng nhập, sử dụng SessionId
                var sessionId = HttpContext.Session.Id;
                return $"{CartSessionKey}_{sessionId}";
            }
        }

        // Phương thức hỗ trợ: Lấy giỏ hàng từ Session
        private ShoppingCart GetCartFromSession()
        {
            var cartKey = GetCartSessionKey();
            return HttpContext.Session.GetObjectFromJson<ShoppingCart>(cartKey)
                   ?? new ShoppingCart();
        }

        // Phương thức hỗ trợ: Lưu giỏ hàng vào Session
        private void SaveCartToSession(ShoppingCart cart)
        {
            var cartKey = GetCartSessionKey();
            HttpContext.Session.SetObjectAsJson(cartKey, cart);
        }

        // Phương thức chuyển giỏ hàng khi user đăng nhập
        public async Task<IActionResult> MergeCartOnLogin()
        {
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    var sessionId = HttpContext.Session.Id;
                    var userId = _userManager.GetUserId(User);

                    var guestCartKey = $"{CartSessionKey}_{sessionId}";
                    var userCartKey = $"{CartSessionKey}_{userId}";

                    // Lấy giỏ hàng của guest
                    var guestCart = HttpContext.Session.GetObjectFromJson<ShoppingCart>(guestCartKey);

                    if (guestCart != null && guestCart.Items.Any())
                    {
                        // Lấy giỏ hàng của user (nếu có)
                        var userCart = HttpContext.Session.GetObjectFromJson<ShoppingCart>(userCartKey)
                                      ?? new ShoppingCart();

                        // Merge giỏ hàng guest vào giỏ hàng user
                        foreach (var item in guestCart.Items)
                        {
                            userCart.AddItem(item);
                        }

                        // Lưu giỏ hàng đã merge
                        HttpContext.Session.SetObjectAsJson(userCartKey, userCart);

                        // Xóa giỏ hàng guest
                        HttpContext.Session.Remove(guestCartKey);
                    }
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}