using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LGBTOUR.Api.Entities
{
    public class Narration
    {
        [Key]
        public int Id { get; set; }

        public int POI_Id { get; set; }
        [ForeignKey("POI_Id")]
        public POI POI { get; set; }

        [Required]
        [MaxLength(10)]
        public string LanguageCode { get; set; } // VD: "vi", "en"

        [Required]
        public string ContentText { get; set; } // Nội dung kịch bản chữ

        // ==========================================
        // CÁC TRƯỜNG TỪ BẢNG AUDIO CŨ CHUYỂN SANG
        // ==========================================

        [MaxLength(500)]
        public string? AudioUrl { get; set; } // Link file âm thanh (Cho phép null nếu chưa có)

        public int DurationSeconds { get; set; } = 0; // Thời lượng file (giây)

        [MaxLength(50)]
        public string? VoiceType { get; set; } // VD: "Giọng Nữ Miền Nam", "AI Text-To-Speech"
    }
}