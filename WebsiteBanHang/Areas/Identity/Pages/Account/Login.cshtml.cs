using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using WebsiteBanHang.Models;
using WebsiteBanHang.Extensions;

namespace WebsiteBanHang.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(SignInManager<ApplicationUser> signInManager, ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public IList<AuthenticationScheme> ExternalLogins { get; set; } = new List<AuthenticationScheme>();

        public string? ReturnUrl { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Email là bắt buộc")]
            [EmailAddress(ErrorMessage = "Email không hợp lệ")]
            [Display(Name = "Email")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
            [DataType(DataType.Password)]
            [Display(Name = "Mật khẩu")]
            public string Password { get; set; } = string.Empty;

            [Display(Name = "Ghi nhớ đăng nhập")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string? returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            // SỬA: Không set returnUrl mặc định
            ReturnUrl = returnUrl;

            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            // SỬA: Bỏ qua returnUrl và luôn về trang chủ
            returnUrl = "/";

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                // Lưu giỏ hàng guest trước khi đăng nhập
                var guestCartKey = $"Cart_{HttpContext.Session.Id}";
                var guestCart = HttpContext.Session.GetObjectFromJson<ShoppingCart>(guestCartKey);

                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");

                    // Merge giỏ hàng sau khi đăng nhập thành công
                    await MergeGuestCartToUserCart(guestCart);

                    // Redirect về trang chủ
                    return Redirect("/");
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không chính xác.");
                    return Page();
                }
            }

            return Page();
        }

        private async Task MergeGuestCartToUserCart(ShoppingCart? guestCart)
        {
            try
            {
                if (guestCart != null && guestCart.Items.Any())
                {
                    var user = await _signInManager.UserManager.GetUserAsync(HttpContext.User);
                    if (user != null)
                    {
                        var userCartKey = $"Cart_{user.Id}";
                        var userCart = HttpContext.Session.GetObjectFromJson<ShoppingCart>(userCartKey)
                                      ?? new ShoppingCart();

                        foreach (var item in guestCart.Items)
                        {
                            userCart.AddItem(item);
                        }

                        HttpContext.Session.SetObjectAsJson(userCartKey, userCart);

                        var guestCartKey = $"Cart_{HttpContext.Session.Id}";
                        HttpContext.Session.Remove(guestCartKey);

                        _logger.LogInformation($"Merged guest cart with {guestCart.Items.Count} items to user cart.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error merging guest cart to user cart.");
            }
        }
    }
}