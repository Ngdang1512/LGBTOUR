using System.ComponentModel.DataAnnotations.Schema;

namespace SaigonAudioTour.Api.Entities
{
    public class TourPOI
    {
        public int Id { get; set; }

        public int TourId { get; set; }
        public Tour Tour { get; set; }//cho phép lấy thông tin từ bảng tour 

        // Chú ý: Đặt tên là POI_Id để khớp 100% với file ApplicationDbContext
        public int POI_Id { get; set; }
        public POI POI { get; set; }

        // Thứ tự của quán trong tuyến (Quán nào đến trước, quán nào đến sau)
        public int DisplayOrder { get; set; }
    }
}