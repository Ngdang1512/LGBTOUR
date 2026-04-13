using System.Threading.Tasks;

namespace SaigonAudioTour.Api.Services
{
    public interface ITtsService
    {
        /// <summary>
        /// Converts text to speech and returns the audio file path
        /// </summary>
        Task<string?> TextToSpeechAsync(string text, string languageCode);
    }
}
