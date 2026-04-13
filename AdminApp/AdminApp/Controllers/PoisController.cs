using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdminApp.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AdminApp.Models;

namespace AdminApp.Controllers
{
    [Authorize]
    public class PoisController : Controller
    {
        private readonly AdminPoiApiClient _poiApiClient;

        public PoisController(AdminPoiApiClient poiApiClient)
        {
            _poiApiClient = poiApiClient;
        }

        private string? GetToken() => User.FindFirst("api_token")?.Value;

        // GET: Pois
        public async Task<IActionResult> Index()
        {
            var token = GetToken();
            if (string.IsNullOrWhiteSpace(token)) return RedirectToAction("Login", "Account");

            var pois = await _poiApiClient.GetAllAsync(token);
            return View(pois);
        }

        // GET: Pois/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var token = GetToken();
            if (string.IsNullOrWhiteSpace(token)) return RedirectToAction("Login", "Account");

            var poi = await _poiApiClient.GetByIdAsync(id.Value, token);
            if (poi == null)
            {
                return NotFound();
            }

            return View(poi);
        }

        // GET: Pois/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Pois/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,Lat,Lng,Radius,Image,AudioPath")] Poi poi)
        {
            var token = GetToken();
            if (string.IsNullOrWhiteSpace(token)) return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                var createdId = await _poiApiClient.CreateAsync(poi, token);
                if (createdId == null || createdId <= 0)
                {
                    ModelState.AddModelError(string.Empty, "Không thể tạo POI. Vui lòng kiểm tra API hoặc quyền admin.");
                    return View(poi);
                }

                TempData["Success"] = "Đã tạo POI. Bạn có thể gắn ảnh và audio ngay tại trang chỉnh sửa.";
                return RedirectToAction(nameof(Edit), new { id = createdId.Value });
            }
            return View(poi);
        }

        // GET: Pois/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var token = GetToken();
            if (string.IsNullOrWhiteSpace(token)) return RedirectToAction("Login", "Account");

            var poi = await _poiApiClient.GetByIdAsync(id.Value, token);
            if (poi == null)
            {
                return NotFound();
            }
            return View(poi);
        }

        // POST: Pois/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Lat,Lng,Radius,Image,AudioPath")] Poi poi)
        {
            if (id != poi.Id)
            {
                return NotFound();
            }

            var token = GetToken();
            if (string.IsNullOrWhiteSpace(token)) return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                var success = await _poiApiClient.UpdateAsync(poi, token);
                if (!success)
                {
                    ModelState.AddModelError(string.Empty, "Không thể cập nhật POI. Vui lòng kiểm tra API hoặc quyền admin.");
                    return View(poi);
                }

                return RedirectToAction(nameof(Index));
            }
            return View(poi);
        }

        // GET: Pois/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var token = GetToken();
            if (string.IsNullOrWhiteSpace(token)) return RedirectToAction("Login", "Account");

            var poi = await _poiApiClient.GetByIdAsync(id.Value, token);
            if (poi == null)
            {
                return NotFound();
            }

            return View(poi);
        }

        // POST: Pois/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var token = GetToken();
            if (string.IsNullOrWhiteSpace(token)) return RedirectToAction("Login", "Account");

            var success = await _poiApiClient.DeleteAsync(id, token);
            if (!success)
            {
                TempData["Error"] = "Không thể xóa POI. Vui lòng thử lại.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadImage(int id, IFormFile imageFile)
        {
            var token = GetToken();
            if (string.IsNullOrWhiteSpace(token)) return RedirectToAction("Login", "Account");

            if (imageFile == null || imageFile.Length == 0)
            {
                TempData["Error"] = "Vui lòng chọn file hình ảnh.";
                return RedirectToAction(nameof(Edit), new { id });
            }

            var success = await _poiApiClient.UploadImageAsync(id, imageFile, token);
            TempData[success ? "Success" : "Error"] = success
                ? "Đã upload hình ảnh thành công."
                : "Upload hình ảnh thất bại.";

            return RedirectToAction(nameof(Edit), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNarration(int id, string languageCode, string contentText, IFormFile? audioFile)
        {
            var token = GetToken();
            if (string.IsNullOrWhiteSpace(token)) return RedirectToAction("Login", "Account");

            if (string.IsNullOrWhiteSpace(languageCode) || string.IsNullOrWhiteSpace(contentText))
            {
                TempData["Error"] = "Vui lòng nhập ngôn ngữ và nội dung thuyết minh.";
                return RedirectToAction(nameof(Edit), new { id });
            }

            var success = await _poiApiClient.CreateNarrationAsync(id, languageCode.Trim(), contentText.Trim(), audioFile, token);
            TempData[success ? "Success" : "Error"] = success
                ? "Đã lưu dữ liệu audio/thuyết minh thành công."
                : "Lưu dữ liệu audio/thuyết minh thất bại.";

            return RedirectToAction(nameof(Edit), new { id });
        }
    }
}
