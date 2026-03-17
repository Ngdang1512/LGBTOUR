namespace LGBTOUR.AdminWeb.Entities
{
    public class Tour
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }

        public virtual ICollection<TourPOI> TourPOIs { get; set; } = new List<TourPOI>();
    }
}
