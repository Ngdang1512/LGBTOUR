using System.Threading.Tasks;

namespace SaigonAudioTour.Api.Services
{
    public class NoopTtsService : ITtsService
    {
        public Task<string?> TextToSpeechAsync(string text, string languageCode)
        {
            return Task.FromResult<string?>(null);
        }
    }
}