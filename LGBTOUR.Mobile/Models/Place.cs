namespace LGBTOUR.Mobile.Models;

public class Place
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Location { get; set; }
    public string ImageUrl { get; set; }
    public string Rating { get; set; }
    public string Category { get; set; }
    
    // Các trường dữ liệu mới phục vụ định vị và Thuyết minh
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int TriggerRadius { get; set; }
    public int Priority { get; set; }
    public string TtsScript { get; set; }
}