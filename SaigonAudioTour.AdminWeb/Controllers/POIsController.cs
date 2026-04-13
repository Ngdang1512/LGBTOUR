using SaigonAudioTour.AdminWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaigonAudioTour.AdminWeb.Models; // Đã sửa theo tên Project mới
using System.Net.Http.Headers;

namespace SaigonAudioTour.AdminWeb.Controllers
{
    [Authorize] // Bắt buộc đăng nhập mới được vào
    public class POIsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public POIsController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // ==========================================
        // 1. XEM DANH SÁCH (READ)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync("api/Pois");

            if (response.IsSuccessStatusCode)
            {
                var pois = await response.Content.ReadFromJsonAsync<List<PoiViewModel>>();
                return View(pois ?? new List<PoiViewModel>());
            }

            // Lỗi API thì trả về mảng rỗng để không bị sập Web
            ViewBag.Error = "Không thể kết nối đến Backend API.";
            return View(new List<PoiViewModel>());
        }

        // ==========================================
        // 2. THÊM MỚI (CREATE)
        // ==========================================
        [HttpGet]
        public IActionResult Create()
        {
            // Trả về form trống với vài thông số mặc định
            return View(new PoiViewModel { Radius = 100, Priority = 1, IsStopStation = true });
        }

        [HttpPost]
        public async Task<IActionResult> Create(PoiViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var client = _httpClientFactory.CreateClient("ApiClient");
            var token = User.FindFirst("JWToken")?.Value;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var createData = new
            {
                name = model.Name,
                description = model.Description,
                lat = model.Lat,
                lng = model.Lng,
                radius = model.Radius,
                priority = model.Priority,
                isStopStation = model.IsStopStation
            };

            var response = await client.PostAsJsonAsync("api/Pois", createData);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Đã thêm Trạm mới thành công!";
                return RedirectToAction("Index");
            }

            ViewBag.Error = "Có lỗi từ API khi thêm dữ liệu!";
            return View(model);
        }

        // ==========================================
        // 3. CẬP NHẬT (UPDATE & UPLOAD ẢNH)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync("api/Pois");

            if (response.IsSuccessStatusCode)
            {
                var pois = await response.Content.ReadFromJsonAsync<List<PoiViewModel>>();
                var poi = pois?.FirstOrDefault(p => p.Id == id);
                if (poi != null) return View(poi);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, PoiViewModel model, IFormFile? uploadImage)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var token = User.FindFirst("JWToken")?.Value;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Bước 1: Cập nhật thông tin chữ và tọa độ
            var updateData = new
            {
                name = model.Name,
                description = model.Description,
                lat = model.Lat,
                lng = model.Lng,
                radius = model.Radius,
                priority = model.Priority,
                isStopStation = model.IsStopStation
            };

            var updateResponse = await client.PutAsJsonAsync($"api/Pois/{id}", updateData);

            if (updateResponse.IsSuccessStatusCode)
            {
                // Bước 2: Nếu có chọn ảnh mới thì gọi API Upload Ảnh
                if (uploadImage != null && uploadImage.Length > 0)
                {
                    using var content = new MultipartFormDataContent();
                    var fileContent = new StreamContent(uploadImage.OpenReadStream());
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue(uploadImage.ContentType);
                    content.Add(fileContent, "imageFile", uploadImage.FileName);

                    await client.PostAsync($"api/Pois/{id}/image", content);
                }

                TempData["SuccessMessage"] = "Cập nhật Trạm thành công!";
                return RedirectToAction("Index");
            }

            ViewBag.Error = "Lỗi hệ thống khi cập nhật thông tin!";
            return View(model);
        }

        // ==========================================
        // 4. XÓA TRẠM (DELETE)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync("api/Pois");

            if (response.IsSuccessStatusCode)
            {
                var pois = await response.Content.ReadFromJsonAsync<List<PoiViewModel>>();
                var poi = pois?.FirstOrDefault(p => p.Id == id);
                if (poi != null) return View(poi);
            }
            return RedirectToAction("Index");
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var token = User.FindFirst("JWToken")?.Value;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.DeleteAsync($"api/Pois/{id}");

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Đã xóa Trạm thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Xóa thất bại! Trạm này có thể đang được sử dụng trong Tuyến xe buýt.";
            }

            return RedirectToAction("Index");
        }
    }
}