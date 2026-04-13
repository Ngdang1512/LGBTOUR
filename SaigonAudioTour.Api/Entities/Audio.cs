using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SaigonAudioTour.Api.Entities
{
    public class Audio
    {
        [Key]
        public int Id { get; set; }

        public int POI_Id { get; set; }
        [ForeignKey("POI_Id")]
        public POI? POI { get; set; }

        [Required]
        [MaxLength(10)]
        public string LanguageCode { get; set; } = "vi"; // VD: "vi", "en", "ko"

        [Required]
        [MaxLength(500)]
        public string AudioUrl { get; set; } = string.Empty; // Link file âm thanh: /audios/xxx.wav

        public int Duration { get; set; } = 0; // Thời lượng file (giây)
    }
}
