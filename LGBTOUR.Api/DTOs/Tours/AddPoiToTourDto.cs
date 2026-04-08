using System.ComponentModel.DataAnnotations;

namespace LGBTOUR.Api.DTOs.Tours
{
    public class AddPoiToTourDto
    {
        [Required]
        public int PoiId { get; set; }

        [Required]
        public int DisplayOrder { get; set; } // Quyết định quán này nằm thứ mấy trong Tour
    }
}