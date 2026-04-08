using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace LGBTOUR.Api.DTOs.Narrations
{
    public class CreateNarrationDto
    {
        [Required]
        public int PoiId { get; set; } // Biết thuyết minh này của quán nào

        [Required]
        [MaxLength(10)]
        public string LanguageCode { get; set; } = string.Empty; // "vi" hoặc "en"

        [Required]
        public string ContentText { get; set; } = string.Empty;

        public string? VoiceType { get; set; } // Vd: "Giọng AI Nữ"

        // IFormFile là kiểu dữ liệu đặc biệt của .NET để hứng file từ Request
        public IFormFile? AudioFile { get; set; }
    }
}