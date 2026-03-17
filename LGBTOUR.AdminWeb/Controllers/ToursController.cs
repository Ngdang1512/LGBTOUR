using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LGBTOUR.AdminWeb.Data;
using LGBTOUR.AdminWeb.Entities; // Hoặc LGBTOUR.Api.Entities tùy cấu trúc của bạn
using LGBTOUR.AdminWeb.ViewModels;

namespace LGBTOUR.AdminWeb.Controllers
{
    public class ToursController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ToursController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. CÁC TÍNH NĂNG QUẢN LÝ CƠ BẢN (CRUD)
        // ==========================================

        // GET: Tours
        public async Task<IActionResult> Index()
        {
            // Kèm theo Include TourPOIs để ngoài View có thể đếm số lượng địa điểm
            var tours = await _context.Tours
                .Include(t => t.TourPOIs)
                .ToListAsync();
            return View(tours);
        }

        // GET: Tours/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var tour = await _context.Tours
                .Include(t => t.TourPOIs)
                .ThenInclude(tp => tp.POI) // Lấy luôn thông tin chi tiết của POI
                .FirstOrDefaultAsync(m => m.Id == id);

            if (tour == null) return NotFound();

            return View(tour);
        }

        // GET: Tours/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Tours/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,Price")] Tour tour)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tour);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm tuyến xe mới thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(tour);
        }

        // GET: Tours/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var tour = await _context.Tours.FindAsync(id);
            if (tour == null) return NotFound();

            return View(tour);
        }

        // POST: Tours/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Price")] Tour tour)
        {
            if (id != tour.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tour);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật thông tin tuyến xe thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TourExists(tour.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(tour);
        }

        // GET: Tours/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var tour = await _context.Tours.FirstOrDefaultAsync(m => m.Id == id);
            if (tour == null) return NotFound();

            return View(tour);
        }

        // POST: Tours/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tour = await _context.Tours.FindAsync(id);
            if (tour != null)
            {
                _context.Tours.Remove(tour);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa tuyến xe khỏi hệ thống!";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool TourExists(int id)
        {
            return _context.Tours.Any(e => e.Id == id);
        }

        // ==========================================
        // 2. TÍNH NĂNG NÂNG CAO: THIẾT LẬP LỘ TRÌNH
        // ==========================================

        // GET: Tours/Itinerary/5
        public async Task<IActionResult> Itinerary(int id)
        {
            // Tìm Tour và nạp sẵn các POI đã được map với Tour này
            var tour = await _context.Tours
                .Include(t => t.TourPOIs)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tour == null) return NotFound();

            // Lấy danh sách toàn bộ các địa điểm (POI) đang có trong hệ thống
            var allPOIs = await _context.POIs.ToListAsync();
            var model = new List<TourItineraryViewModel>();

            foreach (var poi in allPOIs)
            {
                // Kiểm tra xem POI này đã có trong Tour hiện tại chưa
                var tourPoi = tour.TourPOIs.FirstOrDefault(tp => tp.POI_Id == poi.Id);

                model.Add(new TourItineraryViewModel
                {
                    POI_Id = poi.Id,
                    POIName = poi.Name,
                    IsSelected = tourPoi != null,
                    DisplayOrder = tourPoi?.DisplayOrder ?? 0
                });
            }

            ViewBag.TourName = tour.Name;
            ViewBag.TourId = tour.Id;

            // Sắp xếp: Ưu tiên điểm đã chọn lên đầu, sau đó sắp xếp theo thứ tự (DisplayOrder)
            var sortedModel = model.OrderByDescending(m => m.IsSelected)
                                   .ThenBy(m => m.DisplayOrder)
                                   .ToList();

            return View(sortedModel);
        }

        // POST: Tours/UpdateItinerary
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateItinerary(int tourId, List<TourItineraryViewModel> model)
        {
            var tour = await _context.Tours
                .Include(t => t.TourPOIs)
                .FirstOrDefaultAsync(t => t.Id == tourId);

            if (tour == null) return NotFound();

            // Xóa sạch lộ trình cũ của tuyến xe này
            _context.TourPOIs.RemoveRange(tour.TourPOIs);

            // Tìm những điểm người dùng tích chọn (IsSelected = true) để thêm lại
            var selectedItems = model.Where(m => m.IsSelected).ToList();

            foreach (var item in selectedItems)
            {
                var newTourPoi = new TourPOI
                {
                    TourId = tourId,
                    POI_Id = item.POI_Id,
                    DisplayOrder = item.DisplayOrder
                };
                _context.TourPOIs.Add(newTourPoi);
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Cập nhật lộ trình cho tuyến '{tour.Name}' thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}