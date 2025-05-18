using System.ComponentModel.DataAnnotations.Schema;

namespace WebsiteBanHang.Models
{
    public class ProductImage
    {
        public int Id { get; set; }

        public string Url { get; set; }

        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product? Product { get; set; }
    }
}