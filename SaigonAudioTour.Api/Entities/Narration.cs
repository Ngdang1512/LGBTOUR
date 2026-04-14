using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SaigonAudioTour.Api.Entities
{
    public class Narration // Hoặc có thể đổi tên thành PoiTranslation cho đúng ý nghĩa
    {
        [Key]
        public int Id { get; set; }

        public int POI_Id { get; set; }
        [ForeignKey("POI_Id")]
        public POI? POI { get; set; }

        [Required]
        [MaxLength(10)]
        public string LanguageCode { get; set; } // VD: "vi", "en", "ko"

        [Required]
        [StringLength(200)]
        public string TranslatedName { get; set; } // Tên trạm (VD: "Chợ Bến Thành" / "Ben Thanh Market")

        public string? TranslatedDescription { get; set; } // Mô tả trạm bằng ngôn ngữ tương ứng

        public string? ContentText { get; set; } // Nội dung kịch bản chữ dùng cho TTS nếu không có file Audio

        [MaxLength(500)]
        public string? AudioUrl { get; set; } // Link file âm thanh MP3

        public int DurationSeconds { get; set; } = 0; // Thời lượng file (giây)

        [MaxLength(50)]
        public string? VoiceType { get; set; } // VD: "Giọng Nữ Miền Nam", "AI Text-To-Speech"
    }
}