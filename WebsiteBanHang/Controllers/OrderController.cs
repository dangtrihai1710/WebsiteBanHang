// Controllers/OrderController.cs (cho User)
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebsiteBanHang.Models;
using WebsiteBanHang.Repositories;

namespace WebsiteBanHang.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IOrderRepository _orderRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<OrderController> _logger;

        public OrderController(
            IOrderRepository orderRepository,
            UserManager<ApplicationUser> userManager,
            ILogger<OrderController> logger)
        {
            _orderRepository = orderRepository;
            _userManager = userManager;
            _logger = logger;
        }

        // Hiển thị đơn hàng của user hiện tại
        public async Task<IActionResult> Index()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login", "Account", new { area = "Identity" });
                }

                var orders = await _orderRepository.GetByUserIdAsync(user.Id);
                return View(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user orders");
                TempData["ErrorMessage"] = "Lỗi khi tải danh sách đơn hàng: " + ex.Message;
                return View(new List<Order>());
            }
        }

        // Xem chi tiết đơn hàng của user
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login", "Account", new { area = "Identity" });
                }

                var order = await _orderRepository.GetByIdWithDetailsAsync(id);
                if (order == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy đơn hàng!";
                    return RedirectToAction(nameof(Index));
                }

                // Kiểm tra xem đơn hàng có thuộc về user hiện tại không
                if (order.UserId != user.Id)
                {
                    TempData["ErrorMessage"] = "Bạn không có quyền xem đơn hàng này!";
                    return RedirectToAction(nameof(Index));
                }

                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading order details for user");
                TempData["ErrorMessage"] = "Lỗi khi tải chi tiết đơn hàng: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // Hủy đơn hàng (chỉ cho phép khi đơn hàng ở trạng thái "Đang xử lý")
        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Json(new { success = false, message = "Bạn cần đăng nhập để thực hiện thao tác này!" });
                }

                var order = await _orderRepository.GetByIdAsync(id);
                if (order == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đơn hàng!" });
                }

                // Kiểm tra quyền sở hữu
                if (order.UserId != user.Id)
                {
                    return Json(new { success = false, message = "Bạn không có quyền hủy đơn hàng này!" });
                }

                // Kiểm tra trạng thái đơn hàng
                if (order.OrderStatus != "Đang xử lý")
                {
                    return Json(new { success = false, message = "Chỉ có thể hủy đơn hàng đang ở trạng thái 'Đang xử lý'!" });
                }

                // Cập nhật trạng thái
                order.OrderStatus = "Đã hủy";
                await _orderRepository.UpdateAsync(order);

                _logger.LogInformation("User {UserId} cancelled order {OrderId}", user.Id, id);
                return Json(new { success = true, message = "Hủy đơn hàng thành công!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling order {OrderId}", id);
                return Json(new { success = false, message = "Lỗi khi hủy đơn hàng: " + ex.Message });
            }
        }

        // Mua lại đơn hàng (thêm tất cả sản phẩm vào giỏ hàng)
        [HttpPost]
        public async Task<IActionResult> Reorder(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Json(new { success = false, message = "Bạn cần đăng nhập để thực hiện thao tác này!" });
                }

                var order = await _orderRepository.GetByIdWithDetailsAsync(id);
                if (order == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đơn hàng!" });
                }

                // Kiểm tra quyền sở hữu
                if (order.UserId != user.Id)
                {
                    return Json(new { success = false, message = "Bạn không có quyền mua lại đơn hàng này!" });
                }

                // Redirect đến ShoppingCart controller để thêm sản phẩm
                var productIds = order.OrderDetails.Select(od => od.ProductId).ToList();
                var quantities = order.OrderDetails.Select(od => od.Quantity).ToList();

                return Json(new
                {
                    success = true,
                    message = "Đang thêm sản phẩm vào giỏ hàng...",
                    redirect = Url.Action("AddMultipleToCart", "ShoppingCart", new
                    {
                        productIds = string.Join(",", productIds),
                        quantities = string.Join(",", quantities)
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reordering {OrderId}", id);
                return Json(new { success = false, message = "Lỗi khi mua lại đơn hàng: " + ex.Message });
            }
        }
    }
}