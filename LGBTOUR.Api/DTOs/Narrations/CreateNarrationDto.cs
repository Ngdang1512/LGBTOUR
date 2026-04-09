using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace LGBTOUR.Api.DTOs.Narrations
{
    public class CreateNarrationDto
    {
        [Required]
        public int PoiId { get; set; } // Audio này thuộc trạm/địa danh nào

        [Required]
        [MaxLength(10)]
        public string LanguageCode { get; set; } = string.Empty;

        [Required]
        public string ContentText { get; set; } = string.Empty;

        public string? VoiceType { get; set; }

        // Dùng IFormFile để nhận file mp3 từ Frontend truyền lên
        public IFormFile? AudioFile { get; set; }
    }
}