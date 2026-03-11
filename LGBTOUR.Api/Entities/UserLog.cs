namespace LGBTOUR.Api.Entities
{
    public class UserLog
    {
        public long Id { get; set; } // Dùng long vì dữ liệu log rất lớn
        public string UserId { get; set; } = string.Empty; // DeviceId
        public int? POI_Id { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public string EventType { get; set; } = "MOVE"; // MOVE, ENTER, LISTEN
        public int DurationSeconds { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual POI? POI { get; set; }
    }
}