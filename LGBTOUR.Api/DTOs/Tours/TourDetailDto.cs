using System.Collections.Generic;

namespace LGBTOUR.Api.DTOs.Tours
{
    public class TourDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int EstimatedTimeMinutes { get; set; }

        // Danh sách các quán ăn thuộc tour này, đã được sắp xếp
        public List<TourPoiItemDto> Pois { get; set; } = new List<TourPoiItemDto>();
    }

    // Class phụ trợ để hiển thị quán ăn trong 1 tour
    public class TourPoiItemDto
    {
        public int PoiId { get; set; }
        public string PoiName { get; set; } = string.Empty;
        public int DisplayOrder { get; set; } // Quán nào đi trước, quán nào đi sau
    }
}