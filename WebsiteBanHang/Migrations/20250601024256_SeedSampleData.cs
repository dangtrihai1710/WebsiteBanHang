using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebsiteBanHang.Migrations
{
    /// <inheritdoc />
    public partial class SeedSampleData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Thêm 3 danh mục
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Laptop" },
                    { 2, "Điện thoại" },
                    { 3, "Phụ kiện" }
                });

            // Thêm 10 sản phẩm
            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Name", "Price", "Description", "ImageUrl", "CategoryId" },
                values: new object[,]
                {
                    // Laptop (CategoryId = 1)
                    { 1, "MacBook Air M2 13 inch", 25000000m, "MacBook Air M2 13 inch với chip Apple M2 mạnh mẽ, thiết kế siêu mỏng nhẹ. Màn hình Liquid Retina 13.6 inch sắc nét, pin lên đến 18 giờ. Phù hợp cho công việc văn phòng và sáng tạo.", "/images/macbook-air-m2.jpg", 1 },

                    { 2, "Dell XPS 13 Plus", 28000000m, "Dell XPS 13 Plus với thiết kế premium, màn hình OLED 13.4 inch 3.5K. Processor Intel Core i7 thế hệ 12, RAM 16GB, SSD 512GB. Hiệu năng cao cho công việc đa nhiệm.", "/images/dell-xps-13-plus.jpg", 1 },

                    { 3, "ASUS ROG Zephyrus G14", 32000000m, "ASUS ROG Zephyrus G14 - laptop gaming mỏng nhẹ với AMD Ryzen 9, RTX 4060. Màn hình 14 inch QHD 165Hz. Pin lâu, hiệu năng gaming tuyệt vời.", "/images/asus-rog-zephyrus-g14.jpg", 1 },

                    { 4, "Lenovo ThinkPad X1 Carbon", 35000000m, "Lenovo ThinkPad X1 Carbon Gen 11 - laptop doanh nhân cao cấp. Intel Core i7 thế hệ 13, RAM 32GB, SSD 1TB. Bàn phím TrackPoint huyền thoại, độ bền quân đội.", "/images/lenovo-thinkpad-x1-carbon.jpg", 1 },

                    // Điện thoại (CategoryId = 2)  
                    { 5, "iPhone 15 Pro Max", 29000000m, "iPhone 15 Pro Max với chip A17 Pro, camera 48MP ProRAW. Thiết kế titan cao cấp, màn hình 6.7 inch Super Retina XDR. Action Button mới và cổng USB-C.", "/images/iphone-15-pro-max.jpg", 2 },

                    { 6, "Samsung Galaxy S24 Ultra", 31000000m, "Samsung Galaxy S24 Ultra với S Pen tích hợp, camera 200MP zoom 100x. Snapdragon 8 Gen 3, RAM 12GB, bộ nhớ 256GB. Màn hình Dynamic AMOLED 6.8 inch.", "/images/samsung-galaxy-s24-ultra.jpg", 2 },

                    { 7, "Xiaomi 14 Ultra", 22000000m, "Xiaomi 14 Ultra - camera Leica đỉnh cao với cảm biến 1 inch. Snapdragon 8 Gen 3, sạc nhanh 90W. Thiết kế premium, hiệu năng flagship.", "/images/xiaomi-14-ultra.jpg", 2 },

                    { 8, "Google Pixel 8 Pro", 24000000m, "Google Pixel 8 Pro với AI photography tiên tiến. Tensor G3, camera 50MP với tính năng Magic Eraser. Android thuần, cập nhật 7 năm.", "/images/google-pixel-8-pro.jpg", 2 },

                    // Phụ kiện (CategoryId = 3)
                    { 9, "AirPods Pro 2", 6500000m, "AirPods Pro thế hệ 2 với chip H2, chống ồn chủ động nâng cao. Spatial Audio, MagSafe charging case. Chất lượng âm thanh tuyệt vời.", "/images/airpods-pro-2.jpg", 3 },

                    { 10, "Apple Watch Series 9", 9500000m, "Apple Watch Series 9 với chip S9 SiP, màn hình Always-On Retina sáng hơn. Double Tap gesture mới, theo dõi sức khỏe toàn diện.", "/images/apple-watch-series-9.jpg", 3 }
                });

            // Thêm một số hình ảnh sản phẩm bổ sung
            migrationBuilder.InsertData(
                table: "ProductImages",
                columns: new[] { "Id", "Url", "ProductId", "IsMain", "DisplayOrder" },
                values: new object[,]
                {
                    // MacBook Air M2 - thêm ảnh phụ
                    { 1, "/images/macbook-air-m2.jpg", 1, true, 0 },
                    { 2, "/images/macbook-air-m2-side.jpg", 1, false, 1 },
                    { 3, "/images/macbook-air-m2-open.jpg", 1, false, 2 },

                    // iPhone 15 Pro Max - thêm ảnh phụ  
                    { 4, "/images/iphone-15-pro-max.jpg", 5, true, 0 },
                    { 5, "/images/iphone-15-pro-max-back.jpg", 5, false, 1 },
                    { 6, "/images/iphone-15-pro-max-camera.jpg", 5, false, 2 },

                    // Samsung Galaxy S24 Ultra - thêm ảnh phụ
                    { 7, "/images/samsung-galaxy-s24-ultra.jpg", 6, true, 0 },
                    { 8, "/images/samsung-galaxy-s24-ultra-spen.jpg", 6, false, 1 },

                    // AirPods Pro 2 - thêm ảnh phụ
                    { 9, "/images/airpods-pro-2.jpg", 9, true, 0 },
                    { 10, "/images/airpods-pro-2-case.jpg", 9, false, 1 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Xóa dữ liệu ProductImages trước (do có foreign key)
            migrationBuilder.DeleteData(
                table: "ProductImages",
                keyColumn: "Id",
                keyValues: new object[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });

            // Xóa sản phẩm
            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValues: new object[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });

            // Xóa danh mục
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValues: new object[] { 1, 2, 3 });
        }
    }
}