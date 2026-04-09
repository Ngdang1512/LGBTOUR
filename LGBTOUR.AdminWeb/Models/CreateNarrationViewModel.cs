using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace LGBTOUR.AdminWeb.Models
{
    public class CreateNarrationViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn trạm")]
        public int PoiId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngôn ngữ")]
        public string LanguageCode { get; set; } = "vi";

        [Required(ErrorMessage = "Vui lòng nhập kịch bản thuyết minh")]
        public string ContentText { get; set; } = string.Empty;

        public string? VoiceType { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn file MP3/WAV")]
        public IFormFile AudioFile { get; set; }
    }
}