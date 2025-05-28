using System.ComponentModel.DataAnnotations;

namespace WebsiteBanHang.Models
{
    public class CartItem
    {
        public int ProductId { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        public string? ImageUrl { get; set; }

        public decimal Total => Price * Quantity;
    }
}