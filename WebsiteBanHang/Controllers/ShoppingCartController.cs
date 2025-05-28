using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using WebsiteBanHang.Models;
using WebsiteBanHang.Repositories;
using WebsiteBanHang.Extensions;
using WebsiteBanHang.ViewModels;
using Microsoft.AspNetCore.Authorization;

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
        [Authorize]
        public IActionResult Checkout()
        {
            var cart = GetCartFromSession();
            if (!cart.Items.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng của bạn đang trống!";
                return RedirectToAction("Index");
            }

            var model = new CheckoutViewModel
            {
                Cart = cart
            };

            // Nếu user đã đăng nhập, tự động điền thông tin
            if (User.Identity.IsAuthenticated)
            {
                var user = _userManager.GetUserAsync(User).Result;
                if (user != null)
                {
                    model.ShippingAddress = user.Address ?? "";
                    ViewBag.UserFullName = user.FullName;
                }
            }

            return View(model);
        }

        // Xử lý đặt hàng
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
        {
            try
            {
                var cart = GetCartFromSession();
                if (!cart.Items.Any())
                {
                    TempData["ErrorMessage"] = "Giỏ hàng của bạn đang trống!";
                    return RedirectToAction("Index");
                }

                // Loại bỏ validation cho Cart
                ModelState.Remove("Cart");
                ModelState.Remove("Cart.Items");

                if (ModelState.IsValid)
                {
                    var user = await _userManager.GetUserAsync(User);
                    if (user == null)
                    {
                        TempData["ErrorMessage"] = "Bạn cần đăng nhập để đặt hàng!";
                        return RedirectToAction("Login", "Account", new { area = "Identity" });
                    }

                    // Tạo đơn hàng
                    var order = new Order
                    {
                        UserId = user.Id,
                        OrderDate = DateTime.Now,
                        TotalPrice = cart.GetTotalPrice(),
                        ShippingAddress = model.ShippingAddress,
                        Notes = model.Notes
                    };

                    // Tạo chi tiết đơn hàng
                    foreach (var item in cart.Items)
                    {
                        var orderDetail = new OrderDetail
                        {
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            Price = item.Price
                        };
                        order.OrderDetails.Add(orderDetail);
                    }

                    // Lưu đơn hàng
                    await _orderRepository.AddAsync(order);

                    // Xóa giỏ hàng sau khi đặt hàng thành công
                    ClearCartSession();

                    TempData["SuccessMessage"] = "Đặt hàng thành công! Chúng tôi sẽ sớm liên hệ với bạn.";
                    return RedirectToAction("OrderSuccess", new { id = order.Id });
                }

                // Nếu có lỗi, hiển thị lại form với thông tin giỏ hàng
                model.Cart = cart;
                return View("Checkout", model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi đặt hàng: " + ex.Message;
                model.Cart = GetCartFromSession();
                return View("Checkout", model);
            }
        }

        // Trang thành công sau khi đặt hàng
        [Authorize]
        public async Task<IActionResult> OrderSuccess(int id)
        {
            try
            {
                var order = await _orderRepository.GetByIdWithDetailsAsync(id);
                if (order == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy đơn hàng!";
                    return RedirectToAction("Index", "Home");
                }

                // Kiểm tra quyền truy cập
                var user = await _userManager.GetUserAsync(User);
                if (user == null || order.UserId != user.Id)
                {
                    TempData["ErrorMessage"] = "Bạn không có quyền xem đơn hàng này!";
                    return RedirectToAction("Index", "Home");
                }

                return View(order);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        // Thêm nhiều sản phẩm vào giỏ hàng (cho chức năng mua lại)
        [HttpGet]
        public async Task<IActionResult> AddMultipleToCart(string productIds, string quantities)
        {
            try
            {
                if (string.IsNullOrEmpty(productIds) || string.IsNullOrEmpty(quantities))
                {
                    TempData["ErrorMessage"] = "Thông tin sản phẩm không hợp lệ!";
                    return RedirectToAction("Index");
                }

                var idArray = productIds.Split(',').Select(int.Parse).ToArray();
                var qtyArray = quantities.Split(',').Select(int.Parse).ToArray();

                if (idArray.Length != qtyArray.Length)
                {
                    TempData["ErrorMessage"] = "Thông tin sản phẩm không khớp!";
                    return RedirectToAction("Index");
                }

                var cart = GetCartFromSession();
                for (int i = 0; i < idArray.Length; i++)
                {
                    var product = await _productRepository.GetByIdAsync(idArray[i]);
                    if (product != null)
                    {
                        var cartItem = new CartItem
                        {
                            ProductId = product.Id,
                            Name = product.Name,
                            Price = product.Price,
                            Quantity = qtyArray[i],
                            ImageUrl = product.ImageUrl
                        };
                        cart.AddItem(cartItem);
                    }
                }

                SaveCartToSession(cart);
                TempData["SuccessMessage"] = "Đã thêm tất cả sản phẩm vào giỏ hàng!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
                return RedirectToAction("Index");
            }
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

        // Phương thức hỗ trợ: Xóa giỏ hàng khỏi session
        private void ClearCartSession()
        {
            var cartKey = GetCartSessionKey();
            HttpContext.Session.Remove(cartKey);
        }
    }
}