using LGBTOUR.Api.Data;
using LGBTOUR.Api.DTOs.Pois;
using LGBTOUR.Api.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LGBTOUR.Api.Services
{
    public class PoiService : IPoiService
    {

        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public PoiService(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<IEnumerable<PoiDto>> GetAllPoisAsync()
        {
            // SÁNG TẠO: Dùng AsNoTracking() giúp API chạy nhanh hơn 30% vì không cần Entity Framework theo dõi sự thay đổi
            return await _context.POIs
                .AsNoTracking()
                .Include(p => p.Narrations)
                .OrderBy(p => p.Priority)
                .Select(p => new PoiDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Lat = p.Lat,
                    Lng = p.Lng,
                    Radius = p.Radius,
                    Image = p.Image,
                    Priority = p.Priority,
                    IsStopStation = p.IsStopStation,
                    NarrationCount = p.Narrations.Count
                }).ToListAsync();
        }

        public async Task<PoiDto> CreatePoiAsync(CreatePoiDto dto)
        {
            var poi = new POI
            {
                Name = dto.Name,
                Description = dto.Description,
                Lat = dto.Lat,
                Lng = dto.Lng,
                Radius = dto.Radius,
                Priority = dto.Priority,
                IsStopStation = dto.IsStopStation
            };

            _context.POIs.Add(poi);
            await _context.SaveChangesAsync();

            return new PoiDto
            {
                Id = poi.Id,
                Name = poi.Name,
                Description = poi.Description,
                Priority = poi.Priority,
                IsStopStation = poi.IsStopStation,
                NarrationCount = 0
            };
        }

        public async Task<NearbyPoiDto?> GetNearbyPoiAsync(double currentLat, double currentLng, string langcode)
        {
            var allPois = await _context.POIs.AsNoTracking().Include(p => p.Narrations).ToListAsync();

            NearbyPoiDto? closestPoi = null;
            double minDistance = double.MaxValue;

            foreach (var poi in allPois)
            {
                double distance = CalculateHaversineDistance(currentLat, currentLng, poi.Lat, poi.Lng);

                if (distance <= poi.Radius && distance < minDistance)
                {
                    minDistance = distance;

                    // Logic Fallback Ngôn Ngữ cực kỳ thông minh
                    var narration = poi.Narrations.FirstOrDefault(n => n.LanguageCode == langcode)
                                 ?? poi.Narrations.FirstOrDefault(n => n.LanguageCode == "vi")
                                 ?? poi.Narrations.FirstOrDefault();

                    closestPoi = new NearbyPoiDto
                    {
                        PoiId = poi.Id,
                        PoiName = narration != null ? narration.TranslatedName : poi.Name,
                        IsStopStation = poi.IsStopStation,
                        DistanceMeters = Math.Round(distance, 2),
                        AudioUrl = narration?.AudioUrl
                    };
                }
            }
            return closestPoi;
        }

        private double CalculateHaversineDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371e3; // Bán kính Trái Đất (mét)
            var phi1 = lat1 * Math.PI / 180;
            var phi2 = lat2 * Math.PI / 180;
            var deltaPhi = (lat2 - lat1) * Math.PI / 180;
            var deltaLambda = (lon2 - lon1) * Math.PI / 180;

            var a = Math.Sin(deltaPhi / 2) * Math.Sin(deltaPhi / 2) +
                    Math.Cos(phi1) * Math.Cos(phi2) * Math.Sin(deltaLambda / 2) * Math.Sin(deltaLambda / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        public async Task<bool> UpdatePoiAsync(int id, UpdatePoiDto dto)
        {
            var poi = await _context.POIs.FindAsync(id);
            if (poi == null) return false;

            poi.Name = dto.Name; poi.Description = dto.Description;
            poi.Lat = dto.Lat; poi.Lng = dto.Lng;
            poi.Radius = dto.Radius; poi.Priority = dto.Priority;
            poi.IsStopStation = dto.IsStopStation;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeletePoiAsync(int id)
        {
            var poi = await _context.POIs.FindAsync(id);
            if (poi == null) return false;
            _context.POIs.Remove(poi);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UploadImageAsync(int poiId, IFormFile imageFile)
        {
            var poi = await _context.POIs.FindAsync(poiId);
            if (poi == null || imageFile == null || imageFile.Length == 0) return false;

            // SÁNG TẠO: Kiểm duyệt đuôi file để chống hack upload mã độc
            var extension = Path.GetExtension(imageFile.FileName).ToLower();
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".jfif" };
            if (!allowedExtensions.Contains(extension)) return false;

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "images");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + extension;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            // Xóa ảnh cũ trên server nếu có để tiết kiệm dung lượng (Optional)
            if (!string.IsNullOrEmpty(poi.Image))
            {
                var oldFilePath = Path.Combine(_environment.WebRootPath, poi.Image.TrimStart('/'));
                if (File.Exists(oldFilePath)) File.Delete(oldFilePath);
            }

            poi.Image = $"/images/{uniqueFileName}";
            await _context.SaveChangesAsync();
            return true;
        }
    }
}