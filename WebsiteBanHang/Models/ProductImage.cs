using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebsiteBanHang.Models
{
    public class ProductImage
    {
        public int Id { get; set; }

        [Required]
        public string Url { get; set; } = string.Empty;

        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product? Product { get; set; }

        // Thêm thuộc tính để xác định ảnh chính
        public bool IsMain { get; set; } = false;

        // Thứ tự hiển thị ảnh
        public int DisplayOrder { get; set; } = 0;
    }
}