using System.ComponentModel.DataAnnotations;
namespace LGBTOUR.Api.Entities
{
    public class POI
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public double Lat { get; set; }
        public double Lng { get; set; }

        public int Radius { get; set; } // Bán kính kích hoạt (mét)

        public string? Image { get; set; }

        public int Priority { get; set; } // Mức ưu tiên khi các vùng Geofence chồng lấn

        // Quan hệ 1-N (Một POI có nhiều bản dịch và file âm thanh)
        public virtual ICollection<Narration> Narrations { get; set; } = new List<Narration>();
        public virtual ICollection<Audio> Audios { get; set; } = new List<Audio>();
        public virtual ICollection<TourPOI> TourPOIs { get; set; } = new List<TourPOI>();
    }
}