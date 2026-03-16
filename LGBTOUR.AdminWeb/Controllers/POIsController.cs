using Microsoft.AspNetCore.Mvc;

namespace LGBTOUR.AdminWeb.Controllers
{
    public class POIsController : Controller
    {
        // Trang hiển thị danh sách POI (/POIs)
        public IActionResult Index()
        {
            return View();
        }

        // Trang mở form thêm mới có bản đồ (/POIs/Create)
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
    }
}