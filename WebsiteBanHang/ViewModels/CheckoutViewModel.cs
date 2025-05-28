using System.ComponentModel.DataAnnotations;
using WebsiteBanHang.Models;

namespace WebsiteBanHang.ViewModels
{
    public class CheckoutViewModel
    {
        public ShoppingCart Cart { get; set; } = new ShoppingCart();

        [Required(ErrorMessage = "Địa chỉ giao hàng là bắt buộc")]
        [StringLength(500, ErrorMessage = "Địa chỉ không được quá 500 ký tự")]
        public string ShippingAddress { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Ghi chú không được quá 1000 ký tự")]
        public string? Notes { get; set; }
    }
}