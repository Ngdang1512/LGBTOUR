using SaigonAudioTour.AdminWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace SaigonAudioTour.AdminWeb.Controllers
{
    [Authorize]
    public class ToursController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ToursController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // GET: Danh sách Tuyến xe
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync("api/Tours");

            if (response.IsSuccessStatusCode)
            {
                var tours = await response.Content.ReadFromJsonAsync<List<TourViewModel>>();
                return View(tours ?? new List<TourViewModel>());
            }
            return View(new List<TourViewModel>());
        }

        // GET: Form tạo tuyến mới
        [HttpGet]
        public IActionResult Create()
        {
            return View(new TourViewModel { EstimatedTimeMinutes = 60, TicketPrice = 150000 });
        }

        // POST: Lưu tuyến mới
        [HttpPost]
        public async Task<IActionResult> Create(TourViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var client = _httpClientFactory.CreateClient("ApiClient");
            var token = User.FindFirst("JWToken")?.Value;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var createData = new
            {
                name = model.Name,
                description = model.Description,
                estimatedTimeMinutes = model.EstimatedTimeMinutes,
                ticketPrice = model.TicketPrice,
                totalDistanceKm = model.TotalDistanceKm
            };

            var response = await client.PostAsJsonAsync("api/Tours", createData);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }

            ViewBag.Error = "Có lỗi xảy ra khi tạo Tuyến xe mới!";
            return View(model);
        }
        // GET: Lấy thông tin Tuyến xe để Sửa
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync($"api/Tours/{id}");

            if (response.IsSuccessStatusCode)
            {
                var tour = await response.Content.ReadFromJsonAsync<TourViewModel>();
                if (tour != null) return View(tour);
            }
            return RedirectToAction("Index"); // Nếu không tìm thấy thì văng ra ngoài
        }

        // POST: Lưu thông tin Sửa
        [HttpPost]
        public async Task<IActionResult> Edit(int id, TourViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var client = _httpClientFactory.CreateClient("ApiClient");
            var token = User.FindFirst("JWToken")?.Value;
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var updateData = new
            {
                name = model.Name,
                description = model.Description,
                estimatedTimeMinutes = model.EstimatedTimeMinutes,
                ticketPrice = model.TicketPrice,
                totalDistanceKm = model.TotalDistanceKm
            };

            var response = await client.PutAsJsonAsync($"api/Tours/{id}", updateData);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Đã cập nhật Tuyến xe thành công!";
                return RedirectToAction("Index");
            }

            ViewBag.Error = "Có lỗi xảy ra khi cập nhật Tuyến xe!";
            return View(model);
        }

        // GET: Hiển thị form Xác nhận Xóa
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync($"api/Tours/{id}");

            if (response.IsSuccessStatusCode)
            {
                var tour = await response.Content.ReadFromJsonAsync<TourViewModel>();
                if (tour != null) return View(tour);
            }
            return RedirectToAction("Index");
        }

        // POST: Gửi lệnh Xóa xuống API
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var token = User.FindFirst("JWToken")?.Value;
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await client.DeleteAsync($"api/Tours/{id}");

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Đã xóa Tuyến xe thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể xóa Tuyến xe này (có thể do lỗi dữ liệu liên kết)!";
            }
            return RedirectToAction("Index");
        }
    }
}