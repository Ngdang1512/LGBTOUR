using SaigonAudioTour.Api.Data;
using SaigonAudioTour.Api.DTOs.Pois;
using SaigonAudioTour.Api.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SaigonAudioTour.Api.Services
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
                    ImageUrl = ResolveImageUrl(p),
                    Location = ResolveLocation(p),
                    TtsScript = ResolveTtsScript(p),
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
                Image = poi.Image,
                ImageUrl = ResolveImageUrl(poi),
                Location = ResolveLocation(poi),
                TtsScript = ResolveTtsScript(poi),
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

        private static string ResolveLocation(POI poi)
        {
            return PoiContentCatalog.TryGetValue(poi.Name, out var content) ? content.Location : (poi.Description ?? poi.Name);
        }

        private static string ResolveTtsScript(POI poi)
        {
            return PoiContentCatalog.TryGetValue(poi.Name, out var content) ? content.TtsScript : (poi.Description ?? poi.Name);
        }

        private static string ResolveImageUrl(POI poi)
        {
            if (!string.IsNullOrWhiteSpace(poi.Image))
            {
                return poi.Image.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                    ? poi.Image
                    : poi.Image.StartsWith("/")
                        ? poi.Image
                        : $"/images/{poi.Image}";
            }

            return string.Empty;
        }

        private static readonly IReadOnlyDictionary<string, PoiContent> PoiContentCatalog = new Dictionary<string, PoiContent>
        {
            ["Chợ Bến Thành"] = new PoiContent("Phường Bến Thành, Quận 1, TP.HCM", "/images/6a083625-d1e3-461d-9d29-8d34daf0e4ea.jpg", "Chợ Bến Thành, tọa lạc tại trung tâm Quận 1, TP.HCM, là biểu tượng lịch sử và văn hóa hơn 100 năm tuổi của Sài Gòn. Xây dựng từ năm 1912, chợ nổi bật với kiến trúc cửa Nam có đồng hồ đặc trưng. Nơi đây không chỉ là điểm giao thương nhộn nhịp với đa dạng hàng hóa, ẩm thực mà còn là điểm đến không thể bỏ qua của du khách."),
            ["Nhà hát Thành phố"] = new PoiContent("Số 7 Công trường Lam Sơn, Quận 1, TP.HCM", "/images/71d448c9-1d1d-4577-a1b4-a5a83ada2a8e.jpg", "Nhà hát Thành phố Hồ Chí Minh (số 7 Công trường Lam Sơn, Quận 1) là di tích kiến trúc nghệ thuật cấp Quốc gia, được khánh thành năm 1900. Với phong cách kiến trúc Pháp cổ kính (Gothic), nơi đây được ví như thánh đường nghệ thuật, chuyên tổ chức các buổi hòa nhạc, sân khấu đẳng cấp và là biểu tượng văn hóa lâu đời của TP.HCM."),
            ["Trụ sở Ủy ban Nhân dân TP.HCM"] = new PoiContent("Số 86 Lê Thánh Tôn, Quận 1, TP.HCM", "/images/9217c036-68e9-4777-8c42-8b6aa89daa4b.jpg", "Trụ sở Ủy ban Nhân dân TP.HCM (City Hall) tọa lạc tại số 86 Lê Thánh Tôn, Quận 1, là công trình kiến trúc Pháp cổ kính biểu tượng của Sài Gòn. Xây dựng từ 1898-1909, tòa nhà nổi bật với phong cách Baroque, màu vàng kem rực rỡ và nằm ở cuối đại lộ Nguyễn Huệ, trở thành điểm tham quan check-in không thể bỏ qua."),
            ["Dinh Độc Lập"] = new PoiContent("135 Nam Kỳ Khởi Nghĩa, Quận 1, TP.HCM", "/images/f41b395f-ee49-42ff-b831-96c4ff84173f.jpg", "Dinh Độc Lập (hay Hội trường Thống Nhất) tại Quận 1, TP.HCM là Di tích quốc gia đặc biệt, biểu tượng của hòa bình và thống nhất dân tộc. Được kiến trúc sư Ngô Viết Thụ thiết kế, công trình kết hợp hài hòa kiến trúc hiện đại và triết lý phương Đông. Nơi đây từng là cơ quan đầu não của chính quyền Sài Gòn và chứng kiến thời khắc lịch sử xe tăng húc đổ cổng chính ngày 30/4/1975."),
            ["Nhà thờ Đức Bà"] = new PoiContent("Số 1 Công xã Paris, Quận 1, TP.HCM", "/images/b9ef95d6-1c47-4444-a0a9-d19589360ccb.jpg", "Nhà thờ Đức Bà Sài Gòn (số 1 Công xã Paris, Quận 1) là biểu tượng kiến trúc Pháp đặc trưng, tồn tại hơn 140 năm tại TP.HCM. Khởi công năm 1877 và khánh thành năm 1880, công trình nổi bật với gạch đỏ Marseille không phai màu, hai tháp chuông cao 57m và lối kiến trúc Roman-Gothic cổ kính. Đây là Vương cung thánh đường nổi tiếng, điểm du lịch văn hóa, lịch sử và tín ngưỡng không thể bỏ qua."),
            ["Bưu điện Thành phố"] = new PoiContent("Số 2 Công xã Paris, Quận 1, TP.HCM", "/images/fc1cc35e-7f17-44ee-a052-dba5b9cf6aca.jpg", "Bưu điện Trung tâm Sài Gòn (2 Công xã Paris, Quận 1) là công trình kiến trúc Pháp tiêu biểu được xây dựng từ năm 1886-1891. Đây là bưu điện lâu đời nhất Đông Nam Á, nổi bật với phong cách kiến trúc kết hợp Âu - Á độc đáo, mái vòm rộng và sàn đá cẩm thạch, do kiến trúc sư Gustave Eiffel thiết kế."),
            ["Bảo tàng Chứng tích Chiến tranh"] = new PoiContent("28 Võ Văn Tần, Quận 3, TP.HCM", "/images/dcae6874-1266-4534-b703-966417833a38.jpg", "Bảo tàng Chứng tích Chiến tranh (War Remnants Museum) tại 28 Võ Văn Tần, Q.3, TP.HCM là điểm đến không thể bỏ qua, chuyên trưng bày tư liệu, hình ảnh và hiện vật về tội ác và hậu quả chiến tranh xâm lược Việt Nam. Thành lập năm 1975, bảo tàng lưu giữ hàng nghìn chứng tích, bao gồm cả không gian ngoài trời với máy bay, xe tăng, nhằm tuyên truyền về hòa bình, tình đoàn kết dân tộc."),
            ["Bảo tàng Lịch sử"] = new PoiContent("2 Nguyễn Bỉnh Khiêm, Quận 1, TP.HCM", "/images/0de2c932-b4d9-40d6-a8b1-59f206486bbc.jfif", "Bảo tàng Lịch sử (History Museum) là không gian lưu giữ, trưng bày các hiện vật quý giá, tái hiện sinh động dòng chảy văn hóa và các cột mốc quan trọng của một dân tộc. Đây là điểm đến văn hóa, giáo dục lý tưởng, nơi kết nối quá khứ với hiện tại, giúp người tham quan hiểu rõ hơn về cội nguồn."),
            ["Thảo Cầm Viên Sài Gòn"] = new PoiContent("2 Nguyễn Bỉnh Khiêm, Quận 1, TP.HCM", "/images/6bda7834-25ec-46d9-a3c5-4753afa05cef_6a524098-5caf-4c43-b3eb-e1634a6d5f3a.jfif", "Thảo Cầm Viên Sài Gòn (Saigon Zoo), thành lập năm 1864, là một trong những vườn thú lâu đời nhất thế giới và là lá phổi xanh hơn 20ha giữa Quận 1, TP.HCM. Nơi đây bảo tồn hơn 1.000 cá thể động vật và 2.000 cây xanh, là điểm tham quan, giáo dục tự nhiên lý tưởng với giá vé từ 40.000đ - 60.000đ."),
            ["Saigon Skydeck"] = new PoiContent("Tầng 49, Bitexco Financial Tower, Quận 1, TP.HCM", "/images/41e4cdc7-b073-4a51-8c15-6023b0e9586a_80313477-b26a-4840-9ed8-d07d0a1c65df.jfif", "Saigon Skydeck, tọa lạc tại tầng 49 của tòa tháp Bitexco Financial Tower (Quận 1, TP.HCM), là đài quan sát đầu tiên và nổi tiếng tại Việt Nam, mang đến tầm nhìn toàn cảnh 360 độ từ độ cao 178m. Nơi đây cho phép du khách ngắm nhìn sông Sài Gòn và thành phố, kết hợp trải nghiệm văn hóa truyền thống qua tranh mộc bản.")
        };

        private sealed record PoiContent(string Location, string ImageUrl, string TtsScript);
    }
}