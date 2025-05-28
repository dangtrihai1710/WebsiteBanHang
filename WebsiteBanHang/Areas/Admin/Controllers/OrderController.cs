// Areas/Admin/Controllers/OrderController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebsiteBanHang.Models;
using WebsiteBanHang.Repositories;

namespace WebsiteBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class OrderController : Controller
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<OrderController> _logger;

        public OrderController(IOrderRepository orderRepository, ILogger<OrderController> logger)
        {
            _orderRepository = orderRepository;
            _logger = logger;
        }

        // Hiển thị danh sách tất cả đơn hàng
        public async Task<IActionResult> Index()
        {
            try
            {
                var orders = await _orderRepository.GetAllAsync();
                return View(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading orders list");
                TempData["ErrorMessage"] = "Lỗi khi tải danh sách đơn hàng: " + ex.Message;
                return View(new List<Order>());
            }
        }

        // Xem chi tiết đơn hàng
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var order = await _orderRepository.GetByIdWithDetailsAsync(id);
                if (order == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy đơn hàng!";
                    return RedirectToAction(nameof(Index));
                }

                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading order details for ID: {OrderId}", id);
                TempData["ErrorMessage"] = "Lỗi khi tải chi tiết đơn hàng: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // Cập nhật trạng thái đơn hàng
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            try
            {
                var order = await _orderRepository.GetByIdAsync(id);
                if (order == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đơn hàng!" });
                }

                order.OrderStatus = status;
                await _orderRepository.UpdateAsync(order);

                _logger.LogInformation("Order {OrderId} status updated to {Status}", id, status);
                return Json(new { success = true, message = "Cập nhật trạng thái thành công!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status for ID: {OrderId}", id);
                return Json(new { success = false, message = "Lỗi khi cập nhật trạng thái: " + ex.Message });
            }
        }

        // Xóa đơn hàng
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var order = await _orderRepository.GetByIdWithDetailsAsync(id);
                if (order == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy đơn hàng!";
                    return RedirectToAction(nameof(Index));
                }

                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading delete page for order ID: {OrderId}", id);
                TempData["ErrorMessage"] = "Lỗi khi tải trang xóa đơn hàng: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("DeleteConfirmed")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _orderRepository.DeleteAsync(id);
                _logger.LogInformation("Order {OrderId} deleted successfully", id);
                TempData["SuccessMessage"] = "Xóa đơn hàng thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting order ID: {OrderId}", id);
                TempData["ErrorMessage"] = "Lỗi khi xóa đơn hàng: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // API để lấy thống kê đơn hàng
        [HttpGet]
        public async Task<IActionResult> GetOrderStats()
        {
            try
            {
                var totalOrders = await _orderRepository.GetTotalOrdersCountAsync();
                var totalRevenue = await _orderRepository.GetTotalRevenueAsync();
                var recentOrders = await _orderRepository.GetRecentOrdersAsync(5);

                return Json(new
                {
                    success = true,
                    totalOrders = totalOrders,
                    totalRevenue = totalRevenue,
                    recentOrdersCount = recentOrders.Count()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order statistics");
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}