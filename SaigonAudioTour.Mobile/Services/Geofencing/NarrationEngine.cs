using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Media;
using Microsoft.Maui.Storage;
using SaigonAudioTour.Mobile.Models;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SaigonAudioTour.Mobile.Services.Geofencing
{
    /// <summary>
    /// Event args khi narration playback completed
    /// </summary>
    public class NarrationPlaybackEventArgs : EventArgs
    {
        public int PoiId { get; set; }
        public string LanguageCode { get; set; } = string.Empty;
        public double DurationSeconds { get; set; }
    }

    /// <summary>
    /// Engine điều khiển playback logic cho narration
    /// - Check xem đã phát xong POI nào chưa
    /// - Quyết định TTS hay Audio file
    /// - Handle queue narration
    /// </summary>
    public class NarrationEngine
    {
        private readonly NarrationApiService _narrationApiService;

        private bool _isPlaying;
        private CancellationTokenSource? _playCancellation;

        public event EventHandler<NarrationPlaybackEventArgs>? OnPlaybackCompleted;

        public NarrationEngine(NarrationApiService narrationApiService)
        {
            _narrationApiService = narrationApiService;
            _isPlaying = false;
        }

        /// <summary>
        /// Get user's preferred language - stored in Preferences
        /// </summary>
        private string GetUserLanguage()
        {
            return AppLanguageService.GetNarrationLanguage();
        }

        /// <summary>
        /// Play narration cho POI
        /// 1. Fetch narration từ API
        /// 2. Check audio file hoặc TTS script
        /// 3. Handle queue nếu đang phát
        /// </summary>
        public async Task PlayNarrationAsync(Place poi)
        {
            if (_isPlaying)
            {
                // TODO: Add to queue
                return;
            }

            try
            {
                _isPlaying = true;
                _playCancellation = new CancellationTokenSource();

                var language = GetUserLanguage();
                var narration = await _narrationApiService.GetNarrationByPoiAsync(poi.Id, language);

                if (narration == null)
                    return;

                var startTime = DateTime.UtcNow;

                // Priority: Audio file > TTS
                if (!string.IsNullOrEmpty(narration.AudioUrl))
                {
                    // Play audio file
                    await PlayAudioFileAsync(narration.AudioUrl, _playCancellation.Token);
                }
                else if (!string.IsNullOrEmpty(narration.ContentText))
                {
                    // Fallback: TTS
                    await PlayTtsAsync(narration.ContentText, language, _playCancellation.Token);
                }

                var duration = (DateTime.UtcNow - startTime).TotalSeconds;

                // Trigger event
                OnPlaybackCompleted?.Invoke(this, new NarrationPlaybackEventArgs
                {
                    PoiId = poi.Id,
                    LanguageCode = language,
                    DurationSeconds = duration
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[NarrationEngine] Error: {ex.Message}");
            }
            finally
            {
                _isPlaying = false;
                _playCancellation?.Dispose();
            }
        }

        /// <summary>
        /// Play audio file từ URL
        /// </summary>
        private async Task PlayAudioFileAsync(string audioUrl, CancellationToken cancellationToken)
        {
            try
            {
                // Download audio file nếu cần
                var audioPath = audioUrl;

                if (audioUrl.StartsWith("http"))
                {
                    // Download tạm thời
                    using var client = new HttpClient();
                    var data = await client.GetByteArrayAsync(audioUrl, cancellationToken);
                    var cacheDir = FileSystem.CacheDirectory;
                    var fileName = $"narration_{Guid.NewGuid()}.mp3";
                    var filePath = Path.Combine(cacheDir, fileName);

                    await File.WriteAllBytesAsync(filePath, data, cancellationToken);
                    audioPath = filePath;
                }

                // Play using platform-specific MediaElement hoặc native player
                // TODO: Integrate media player
                await Task.Delay(2000); // Placeholder - simulate playback
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[NarrationEngine] Audio playback error: {ex.Message}");
            }
        }

        /// <summary>
        /// Play TTS từ text content
        /// </summary>
        private async Task PlayTtsAsync(string text, string languageCode, CancellationToken cancellationToken)
        {
            try
            {
                await TextToSpeech.Default.SpeakAsync(
                    text,
                    new SpeechOptions
                    {
                        Locale = GetTtsLocale(languageCode),
                        Volume = 1.0f,
                        Pitch = 1.0f
                    },
                    cancellationToken);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[NarrationEngine] TTS error: {ex.Message}");
            }
        }

        /// <summary>
        /// Map language code thành TTS locale
        /// </summary>
        private static Locale? GetTtsLocale(string languageCode)
        {
            var locales = TextToSpeech.Default.GetLocalesAsync().GetAwaiter().GetResult();
            var preferred = locales.FirstOrDefault(locale =>
                locale.Language.StartsWith(languageCode, StringComparison.OrdinalIgnoreCase));

            if (preferred != null)
            {
                return preferred;
            }

            return locales.FirstOrDefault(locale => locale.Language.StartsWith("en", StringComparison.OrdinalIgnoreCase));
        }

        public void StopPlayback()
        {
            _playCancellation?.Cancel();
            _isPlaying = false;
        }

        public bool IsPlaying => _isPlaying;
    }
}
