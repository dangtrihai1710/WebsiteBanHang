using Microsoft.AspNetCore.Mvc;

namespace WebsiteBanHang.Controllers
{
    public class AccessDeniedController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}