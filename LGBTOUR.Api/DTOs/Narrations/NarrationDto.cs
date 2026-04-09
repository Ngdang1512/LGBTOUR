namespace LGBTOUR.Api.DTOs.Narrations
{
    public class NarrationDto
    {
        public int Id { get; set; }
        public string LanguageCode { get; set; } = string.Empty; // "vi", "en"
        public string ContentText { get; set; } = string.Empty;
        public string? AudioUrl { get; set; }
        public int DurationSeconds { get; set; }
        public string? VoiceType { get; set; }

        // Added so DTO can carry the translated display name if needed by callers
        public string TranslatedName { get; set; } = string.Empty;
    }
}