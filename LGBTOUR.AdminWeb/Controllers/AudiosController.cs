using LGBTOUR.AdminWeb.Data;
using LGBTOUR.AdminWeb.Entities; // Nhớ đổi lại thành LGBTOUR.Api.Entities nếu bạn vẫn dùng namespace cũ
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LGBTOUR.AdminWeb.Controllers
{
    [Authorize]

    public class AudiosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AudiosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Audios
        public async Task<IActionResult> Index()
        {
            // Include thêm bảng POI để ở danh sách ta có thể in ra tên địa điểm thay vì số ID
            var audios = await _context.Audios
                .Include(a => a.POI)
                .ToListAsync();
            return View(audios);
        }

        // GET: Audios/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var audio = await _context.Audios
                .Include(a => a.POI)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (audio == null) return NotFound();

            return View(audio);
        }

        // GET: Audios/Create
        public IActionResult Create()
        {
            // Truyền danh sách các Điểm tham quan (POI) ra View để làm thẻ <select>
            ViewData["POI_Id"] = new SelectList(_context.POIs, "Id", "Name");
            return View();
        }

        // POST: Audios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Bỏ 'AudioUrl' khỏi Bind vì URL sẽ được tạo tự động sau khi upload file. Thêm tham số 'uploadFile'
        public async Task<IActionResult> Create([Bind("Id,POI_Id,LanguageCode,Duration")] Audio audio, IFormFile uploadFile)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra xem Admin có chọn file mp3 chưa
                if (uploadFile != null && uploadFile.Length > 0)
                {
                    // 1. Chỉ định thư mục lưu: wwwroot/uploads/audios/
                    var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "audios");

                    // Nếu thư mục chưa tồn tại thì tạo mới
                    if (!Directory.Exists(uploadDir))
                    {
                        Directory.CreateDirectory(uploadDir);
                    }

                    // 2. Tạo tên file mới (tránh bị trùng tên file cũ)
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(uploadFile.FileName);
                    var filePath = Path.Combine(uploadDir, fileName);

                    // 3. Copy file lên Server
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await uploadFile.CopyToAsync(fileStream);
                    }

                    // 4. Gán đường dẫn để lưu vào Database
                    audio.AudioUrl = "/uploads/audios/" + fileName;
                }
                else
                {
                    // Bắt buộc phải có file mới cho lưu
                    ModelState.AddModelError("AudioUrl", "Vui lòng đính kèm file âm thanh (.mp3, .wav)");
                    ViewData["POI_Id"] = new SelectList(_context.POIs, "Id", "Name", audio.POI_Id);
                    return View(audio);
                }

                _context.Add(audio);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Tải file âm thanh thành công!";
                return RedirectToAction(nameof(Index));
            }

            // Nếu form có lỗi chữ, trả lại form và giữ nguyên SelectList
            ViewData["POI_Id"] = new SelectList(_context.POIs, "Id", "Name", audio.POI_Id);
            return View(audio);
        }

        // GET: Audios/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var audio = await _context.Audios.FindAsync(id);
            if (audio == null) return NotFound();

            ViewData["POI_Id"] = new SelectList(_context.POIs, "Id", "Name", audio.POI_Id);
            return View(audio);
        }

        // POST: Audios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,POI_Id,LanguageCode,AudioUrl,Duration")] Audio audio, IFormFile uploadFile)
        {
            if (id != audio.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // NẾU ADMIN CÓ CHỌN FILE MỚI -> UPLOAD ĐÈ
                    if (uploadFile != null && uploadFile.Length > 0)
                    {
                        var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "audios");
                        if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(uploadFile.FileName);
                        var filePath = Path.Combine(uploadDir, fileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await uploadFile.CopyToAsync(fileStream);
                        }

                        audio.AudioUrl = "/uploads/audios/" + fileName;
                    }
                    // Nếu không chọn file mới, giữ nguyên URL cũ (thuộc tính AudioUrl ở form ẩn gửi lên)

                    _context.Update(audio);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật âm thanh thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AudioExists(audio.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["POI_Id"] = new SelectList(_context.POIs, "Id", "Name", audio.POI_Id);
            return View(audio);
        }

        // GET: Audios/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var audio = await _context.Audios
                .Include(a => a.POI)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (audio == null) return NotFound();

            return View(audio);
        }

        // POST: Audios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var audio = await _context.Audios.FindAsync(id);
            if (audio != null)
            {
                // Bạn có thể viết thêm code xóa file mp3 vật lý trong thư mục wwwroot ở đây nếu muốn hệ thống sạch sẽ
                _context.Audios.Remove(audio);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa file âm thanh!";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool AudioExists(int id)
        {
            return _context.Audios.Any(e => e.Id == id);
        }
    }
}