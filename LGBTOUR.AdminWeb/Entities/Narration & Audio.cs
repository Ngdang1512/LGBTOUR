namespace LGBTOUR.AdminWeb.Entities
{
    public class Narration
    {
        public int Id { get; set; }
        public int POI_Id { get; set; }
        public string LanguageCode { get; set; } = "vi"; // vi, en, kr...
        public string Content { get; set; } = string.Empty;

        public virtual POI? POI { get; set; }
    }

    public class Audio
    {
        public int Id { get; set; }
        public int POI_Id { get; set; }
        public string LanguageCode { get; set; } = "vi";
        public string AudioUrl { get; set; } = string.Empty;
        public int Duration { get; set; } // Độ dài (giây) để phục vụ Analytics

        public virtual POI? POI { get; set; }
    }
}