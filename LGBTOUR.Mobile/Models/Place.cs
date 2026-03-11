namespace LGBTOUR.Mobile.Models;

public class Place
{
    public string Name { get; set; }
    public string Location { get; set; }
    public string ImageUrl { get; set; }
    public string Rating { get; set; }
    public string Category { get; set; } // Dùng để lọc: "Popular", "Food", "Shopping"
}