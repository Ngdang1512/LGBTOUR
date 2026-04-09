using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
namespace LGBTOUR.Api.Entities
{
    public class POI
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        public string? Description { get; set; }

        public double Lat { get; set; } //vĩ độ 
        public double Lng { get; set; }//kinh độ 

        public int Radius { get; set; } //bán kính kích hoạt 

        public string? Image { get; set; } //ảnh 

        public int Priority { get; set; } // độ ưu tiên 
        public bool IsStopStation { get; set; } // Đánh dấu Trạm xe buýt

        public ICollection<Narration> Narrations { get; set; }
        
        public ICollection<TourPOI> TourPOIs { get; set; }
        public ICollection<UserLog> UserLogs { get; set; }

        public POI()
        {
            Name = string.Empty;
            Narrations = new List<Narration>();
            TourPOIs = new List<TourPOI>();
            UserLogs = new List<UserLog>();
        }
    }
}