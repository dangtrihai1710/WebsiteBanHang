// Models/Order.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace WebsiteBanHang.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public DateTime OrderDate { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Tổng tiền phải lớn hơn 0")]
        public decimal TotalPrice { get; set; }

        [Required]
        [StringLength(500, ErrorMessage = "Địa chỉ giao hàng không được quá 500 ký tự")]
        public string ShippingAddress { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Ghi chú không được quá 1000 ký tự")]
        public string? Notes { get; set; }

        public string OrderStatus { get; set; } = "Đang xử lý";

        [Required]
        [StringLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string CustomerPhone { get; set; } = string.Empty;

        public string PaymentMethod { get; set; } = "COD";

        // Navigation properties
        [ForeignKey("UserId")]
        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; }

        public List<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

        // Calculated properties
        public int TotalItems => OrderDetails?.Sum(od => od.Quantity) ?? 0;
    }
}

