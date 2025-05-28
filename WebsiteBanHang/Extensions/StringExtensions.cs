namespace WebsiteBanHang.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Cắt chuỗi theo độ dài chỉ định và thêm "..." nếu cần
        /// </summary>
        /// <param name="value">Chuỗi gốc</param>
        /// <param name="maxLength">Độ dài tối đa</param>
        /// <returns>Chuỗi đã được cắt</returns>
        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            if (value.Length <= maxLength)
                return value;

            return value.Substring(0, maxLength) + "...";
        }

        /// <summary>
        /// Chuyển đổi chuỗi thành slug (URL-friendly)
        /// </summary>
        /// <param name="value">Chuỗi gốc</param>
        /// <returns>Slug</returns>
        public static string ToSlug(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            // Chuyển về chữ thường
            value = value.ToLowerInvariant();

            // Thay thế ký tự đặc biệt tiếng Việt
            value = value.Replace("à", "a").Replace("á", "a").Replace("ạ", "a").Replace("ả", "a").Replace("ã", "a")
                        .Replace("â", "a").Replace("ầ", "a").Replace("ấ", "a").Replace("ậ", "a").Replace("ẩ", "a").Replace("ẫ", "a")
                        .Replace("ă", "a").Replace("ằ", "a").Replace("ắ", "a").Replace("ặ", "a").Replace("ẳ", "a").Replace("ẵ", "a")
                        .Replace("è", "e").Replace("é", "e").Replace("ẹ", "e").Replace("ẻ", "e").Replace("ẽ", "e")
                        .Replace("ê", "e").Replace("ề", "e").Replace("ế", "e").Replace("ệ", "e").Replace("ể", "e").Replace("ễ", "e")
                        .Replace("ì", "i").Replace("í", "i").Replace("ị", "i").Replace("ỉ", "i").Replace("ĩ", "i")
                        .Replace("ò", "o").Replace("ó", "o").Replace("ọ", "o").Replace("ỏ", "o").Replace("õ", "o")
                        .Replace("ô", "o").Replace("ồ", "o").Replace("ố", "o").Replace("ộ", "o").Replace("ổ", "o").Replace("ỗ", "o")
                        .Replace("ơ", "o").Replace("ờ", "o").Replace("ớ", "o").Replace("ợ", "o").Replace("ở", "o").Replace("ỡ", "o")
                        .Replace("ù", "u").Replace("ú", "u").Replace("ụ", "u").Replace("ủ", "u").Replace("ũ", "u")
                        .Replace("ư", "u").Replace("ừ", "u").Replace("ứ", "u").Replace("ự", "u").Replace("ử", "u").Replace("ữ", "u")
                        .Replace("ỳ", "y").Replace("ý", "y").Replace("ỵ", "y").Replace("ỷ", "y").Replace("ỹ", "y")
                        .Replace("đ", "d");

            // Xóa ký tự đặc biệt và thay bằng dấu gạch ngang
            value = System.Text.RegularExpressions.Regex.Replace(value, @"[^a-z0-9\s-]", "");
            value = System.Text.RegularExpressions.Regex.Replace(value, @"\s+", "-");
            value = System.Text.RegularExpressions.Regex.Replace(value, @"-+", "-");
            value = value.Trim('-');

            return value;
        }

        /// <summary>
        /// Kiểm tra xem chuỗi có phải email hợp lệ không
        /// </summary>
        /// <param name="email">Email cần kiểm tra</param>
        /// <returns>True nếu hợp lệ</returns>
        public static bool IsValidEmail(this string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var emailRegex = new System.Text.RegularExpressions.Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                return emailRegex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Chuyển đổi số điện thoại về định dạng chuẩn
        /// </summary>
        /// <param name="phoneNumber">Số điện thoại gốc</param>
        /// <returns>Số điện thoại đã format</returns>
        public static string FormatPhoneNumber(this string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return phoneNumber;

            // Xóa tất cả ký tự không phải số
            var digitsOnly = System.Text.RegularExpressions.Regex.Replace(phoneNumber, @"[^\d]", "");

            // Nếu bắt đầu bằng 84, thay thế bằng 0
            if (digitsOnly.StartsWith("84") && digitsOnly.Length == 11)
                digitsOnly = "0" + digitsOnly.Substring(2);

            // Format theo định dạng Việt Nam
            if (digitsOnly.Length == 10 && digitsOnly.StartsWith("0"))
            {
                return $"{digitsOnly.Substring(0, 4)} {digitsOnly.Substring(4, 3)} {digitsOnly.Substring(7, 3)}";
            }

            return phoneNumber; // Trả về nguyên gốc nếu không đúng format
        }

        /// <summary>
        /// Làm sạch chuỗi HTML
        /// </summary>
        /// <param name="html">Chuỗi HTML</param>
        /// <returns>Chuỗi text thuần</returns>
        public static string StripHtml(this string html)
        {
            if (string.IsNullOrEmpty(html))
                return string.Empty;

            return System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", string.Empty);
        }
    }
}