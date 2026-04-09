using LGBTOUR.AdminWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace LGBTOUR.AdminWeb.Controllers
{
    [Authorize] // Bắt buộc đăng nhập mới được vào
    public class PoisController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public PoisController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // GET: Hiển thị danh sách POIs
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("ApiClient");

            // Gọi API lấy danh sách trạm (API này hôm bữa mình để AllowAnonymous nên gọi thẳng không cần Token)
            var response = await client.GetAsync("api/Pois");

            if (response.IsSuccessStatusCode)
            {
                var pois = await response.Content.ReadFromJsonAsync<List<PoiViewModel>>();
                return View(pois);
            }

            // Nếu lỗi API, trả về mảng rỗng để web không bị sập
            return View(new List<PoiViewModel>());
        }
        // GET: Hiển thị form Thêm mới
        [HttpGet]
        public IActionResult Create()
        {
            // Trả về view kèm vài giá trị mặc định cho tiện
            return View(new PoiViewModel { Radius = 100, Priority = 1, IsStopStation = true });
        }

        // POST: Xử lý khi bấm nút Lưu
        [HttpPost]
        public async Task<IActionResult> Create(PoiViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var client = _httpClientFactory.CreateClient("ApiClient");

            // 1. Móc cái Token từ Cookie ra để xin phép API
            var token = User.FindFirst("JWToken")?.Value;
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // 2. Gói dữ liệu lại cho khớp với CreatePoiDto của API
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

            // 3. Bắn sang API
            var response = await client.PostAsJsonAsync("api/Pois", createData);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index"); // Thành công thì về trang danh sách
            }

            ViewBag.Error = "Có lỗi từ API, vui lòng kiểm tra lại!";
            return View(model);
        }
        // GET: Lấy thông tin trạm cũ đưa lên Form
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
            return RedirectToAction("Index"); // Không tìm thấy thì đá về danh sách
        }

        // POST: Lưu thông tin mới
        [HttpPost]
        public async Task<IActionResult> Edit(int id, PoiViewModel model, IFormFile? uploadImage)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var token = User.FindFirst("JWToken")?.Value;
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // 1. Gọi API PUT để cập nhật thông tin chữ / tọa độ
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
                // 2. LOGIC NÂNG CAO: Nếu Admin chọn ảnh mới, gọi thêm API Upload
                if (uploadImage != null && uploadImage.Length > 0)
                {
                    using var content = new MultipartFormDataContent();
                    var fileContent = new StreamContent(uploadImage.OpenReadStream());
                    fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(uploadImage.ContentType);
                    content.Add(fileContent, "imageFile", uploadImage.FileName);

                    await client.PostAsync($"api/Pois/{id}/image", content);
                }

                TempData["SuccessMessage"] = "Cập nhật Trạm thành công!";
                return RedirectToAction("Index");
            }

            ViewBag.Error = "Lỗi hệ thống khi cập nhật thông tin!";
            return View(model);
        }
        // GET: Hiển thị trang Xác nhận Xóa
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync("api/Pois");

            if (response.IsSuccessStatusCode)
            {
                var pois = await response.Content.ReadFromJsonAsync<List<PoiViewModel>>();
                var poi = pois?.FirstOrDefault(p => p.Id == id);

                // Nếu tìm thấy trạm thì đưa dữ liệu sang View để hiển thị
                if (poi != null) return View(poi);
            }

            // Lỗi hoặc không thấy thì đá về trang chủ
            return RedirectToAction("Index");
        }

        // POST: Xử lý gửi lệnh Xóa xuống API
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");

            // Lấy Token từ Cookie nhét vào Header
            var token = User.FindFirst("JWToken")?.Value;
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Gọi API DELETE
            var response = await client.DeleteAsync($"api/Pois/{id}");

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Đã xóa Trạm thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Xóa thất bại! Trạm này có thể đang được sử dụng trong một Tuyến xe buýt.";
            }

            return RedirectToAction("Index");
        }
    }
}