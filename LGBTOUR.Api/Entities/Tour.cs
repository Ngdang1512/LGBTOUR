using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace LGBTOUR.Api.Entities
{
    public class Tour
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(200)]
        public string Name { get; set; }
        [MaxLength(1000)]
        public string? Description { get; set; }
        [MaxLength(1000)]
        public int EstimatedTimeMinutes { get; set; } // Thời gian dự kiến đi bộ (Phút)

        // Navigation property
        public ICollection<TourPOI> TourPOIs { get; set; }
    }
}
