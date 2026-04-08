using System.ComponentModel.DataAnnotations;

namespace LGBTOUR.Api.DTOs.Tours
{
    public class CreateTourDto
    {
        [Required(ErrorMessage = "Tên Tour không được để trống")]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int EstimatedTimeMinutes { get; set; }
    }
}