using System.ComponentModel.DataAnnotations;

namespace SaigonAudioTour.Api.DTOs.Tours
{
    public class AddPoiToTourDto
    {
        [Required]
        public int PoiId { get; set; }

        [Required]
        public int DisplayOrder { get; set; } // Quyết định xe buýt tới trạm này thứ mấy
    }
}