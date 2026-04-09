using System.ComponentModel.DataAnnotations.Schema;

namespace SaigonAudioTour.Api.Entities
{
    public class UserLog
    {
        // Đã nâng cấp từ int lên long để khớp với BIGINT trong SQL
        public long Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        // Giữ nguyên 2 cột này để dỗ dành SQL Server
        public int POI_Id { get; set; }
        public int POIId { get; set; }

        public double? Lat { get; set; }
        public double? Lng { get; set; }
        public string EventType { get; set; } = "Nghe Audio";

        // Đã nâng cấp từ int? lên long? cho an toàn tuyệt đối
        public long? DurationSeconds { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("POI_Id")]
        public virtual POI? POI { get; set; }
    }
}