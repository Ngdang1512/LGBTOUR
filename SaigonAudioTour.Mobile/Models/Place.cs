namespace SaigonAudioTour.Mobile.Models;

public class Place
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Location { get; set; } = "";
    public string ImageUrl { get; set; } = "";
    public string Rating { get; set; } = "0";
    public string Category { get; set; } = "";
    
    // Các trường dữ liệu mới phục vụ định vị và Thuyết minh
    public double Latitude { get; set; } = 0;
    public double Longitude { get; set; } = 0;
    public int TriggerRadius { get; set; } = 30;
    public int Priority { get; set; } = 0;
    public string TtsScript { get; set; } = "";
    public bool IsNarrating { get; set; } = false;
}