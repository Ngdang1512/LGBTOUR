using SaigonAudioTour.AdminWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net.Http.Headers;

namespace SaigonAudioTour.AdminWeb.Controllers
{
    [Authorize]
    public class NarrationsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public NarrationsController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var client = _httpClientFactory.CreateClient("ApiClient");

            // Lấy danh sách POI để đổ vào Dropdown List
            var response = await client.GetAsync("api/Pois");
            var pois = new List<PoiViewModel>();
            if (response.IsSuccessStatusCode)
            {
                pois = await response.Content.ReadFromJsonAsync<List<PoiViewModel>>() ?? new List<PoiViewModel>();
            }

            ViewBag.PoiList = new SelectList(pois, "Id", "Name");
            return View(new CreateNarrationViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateNarrationViewModel model)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");

            // 1. Lấy thẻ bài Token từ Cookie
            var token = User.FindFirst("JWToken")?.Value;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // 2. Đóng gói dữ liệu thành Form-Data (bắt buộc để gửi File)
            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(model.PoiId.ToString()), "PoiId");
            content.Add(new StringContent(model.LanguageCode), "LanguageCode");
            content.Add(new StringContent(model.ContentText), "ContentText");
            content.Add(new StringContent(model.VoiceType ?? "Người thật thu âm"), "VoiceType");

            // 3. Đính kèm File Audio
            if (model.AudioFile != null)
            {
                var fileContent = new StreamContent(model.AudioFile.OpenReadStream());
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(model.AudioFile.ContentType);
                content.Add(fileContent, "AudioFile", model.AudioFile.FileName);
            }

            // 4. Bắn sang API
            var response = await client.PostAsync("api/Narrations", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Upload file thuyết minh âm thanh thành công!";
                return RedirectToAction("Index", "Pois"); // Chuyển tạm về trang danh sách trạm
            }

            ViewBag.Error = "Có lỗi xảy ra khi upload file lên Server!";

            // Load lại danh sách trạm nếu bị lỗi
            var poiRes = await client.GetAsync("api/Pois");
            if (poiRes.IsSuccessStatusCode)
            {
                var pois = await poiRes.Content.ReadFromJsonAsync<List<PoiViewModel>>();
                ViewBag.PoiList = new SelectList(pois, "Id", "Name", model.PoiId);
            }
            return View(model);
        }
    }
}