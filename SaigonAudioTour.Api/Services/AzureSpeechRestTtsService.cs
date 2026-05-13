using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SaigonAudioTour.Api.Services
{
    public class AzureSpeechRestTtsService : ITtsService
    {
        private const string DefaultVoice = "en-US-JennyNeural";
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        private readonly HttpClient _httpClient;
        private readonly ILogger<AzureSpeechRestTtsService> _logger;

        public AzureSpeechRestTtsService(
            IConfiguration configuration,
            IWebHostEnvironment environment,
            HttpClient httpClient,
            ILogger<AzureSpeechRestTtsService> logger)
        {
            _configuration = configuration;
            _environment = environment;
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<string?> TextToSpeechAsync(string text, string languageCode)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            var provider = _configuration["Tts:Provider"]?.Trim().ToLowerInvariant();
            if (provider != "azure")
            {
                return null;
            }

            var region = _configuration["Tts:Azure:Region"]?.Trim();
            var subscriptionKey = _configuration["Tts:Azure:SubscriptionKey"]?.Trim();

            if (string.IsNullOrWhiteSpace(region) || string.IsNullOrWhiteSpace(subscriptionKey))
            {
                _logger.LogWarning("Azure TTS is not configured. Missing region or subscription key.");
                return null;
            }

            var normalizedLanguage = NormalizeLanguage(languageCode);
            var voiceName = ResolveVoiceName(normalizedLanguage);
            var outputFormat = "audio-16khz-128kbitrate-mono-mp3";
            var cacheFilePath = ResolveCachePath(text, normalizedLanguage, voiceName, outputFormat);

            if (File.Exists(cacheFilePath))
            {
                return ToWebPath(cacheFilePath);
            }

            try
            {
                var token = await RequestAccessTokenAsync(region, subscriptionKey);
                var synthUrl = $"https://{region}.tts.speech.microsoft.com/cognitiveservices/v1";
                var ssml = BuildSsml(text, normalizedLanguage, voiceName);

                using var request = new HttpRequestMessage(HttpMethod.Post, synthUrl);
                request.Headers.Add("Authorization", $"Bearer {token}");
                request.Headers.Add("X-Microsoft-OutputFormat", outputFormat);
                request.Headers.Add("User-Agent", "SaigonAudioTour.Api");
                request.Content = new StringContent(ssml, Encoding.UTF8, "application/ssml+xml");

                using var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Azure TTS failed: {StatusCode} {Body}", (int)response.StatusCode, errorBody);
                    return null;
                }

                var audioBytes = await response.Content.ReadAsByteArrayAsync();
                if (audioBytes.Length == 0)
                {
                    return null;
                }

                var cacheDirectory = Path.GetDirectoryName(cacheFilePath);
                if (!string.IsNullOrWhiteSpace(cacheDirectory))
                {
                    Directory.CreateDirectory(cacheDirectory);
                }

                await File.WriteAllBytesAsync(cacheFilePath, audioBytes);
                return ToWebPath(cacheFilePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Azure TTS synthesis failed.");
                return null;
            }
        }

        private async Task<string> RequestAccessTokenAsync(string region, string subscriptionKey)
        {
            var tokenUrl = $"https://{region}.api.cognitive.microsoft.com/sts/v1.0/issueToken";

            using var request = new HttpRequestMessage(HttpMethod.Post, tokenUrl);
            request.Headers.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
            request.Content = new StringContent(string.Empty, Encoding.UTF8, "application/x-www-form-urlencoded");

            using var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadAsStringAsync()).Trim();
        }

        private string ResolveCachePath(string text, string languageCode, string voiceName, string outputFormat)
        {
            var root = _environment.WebRootPath ?? _environment.ContentRootPath;
            var cacheDirectory = Path.Combine(root, "audios", "tts", languageCode);
            Directory.CreateDirectory(cacheDirectory);

            var version = "v1";
            var input = $"{version}|{languageCode}|{voiceName}|{outputFormat}|{NormalizeText(text)}";
            var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(input))).ToLowerInvariant();
            return Path.Combine(cacheDirectory, $"{hash}.mp3");
        }

        private static string NormalizeLanguage(string languageCode)
        {
            if (string.IsNullOrWhiteSpace(languageCode))
            {
                return "vi-vn";
            }

            var normalized = languageCode.Trim().ToLowerInvariant();
            return normalized switch
            {
                "vi" => "vi-vn",
                "en" => "en-us",
                "ja" => "ja-jp",
                "ko" => "ko-kr",
                "zh" => "zh-cn",
                "fr" => "fr-fr",
                _ => normalized.Contains('-') ? normalized : "en-us"
            };
        }

        private static string ResolveVoiceName(string normalizedLanguage)
        {
            if (normalizedLanguage.StartsWith("vi", StringComparison.OrdinalIgnoreCase))
            {
                return "vi-VN-HoaiMyNeural";
            }

            if (normalizedLanguage.StartsWith("ja", StringComparison.OrdinalIgnoreCase))
            {
                return "ja-JP-NanamiNeural";
            }

            if (normalizedLanguage.StartsWith("ko", StringComparison.OrdinalIgnoreCase))
            {
                return "ko-KR-SunHiNeural";
            }

            if (normalizedLanguage.StartsWith("fr", StringComparison.OrdinalIgnoreCase))
            {
                return "fr-FR-DeniseNeural";
            }

            if (normalizedLanguage.StartsWith("zh", StringComparison.OrdinalIgnoreCase))
            {
                return "zh-CN-XiaoxiaoNeural";
            }

            return normalizedLanguage.StartsWith("en", StringComparison.OrdinalIgnoreCase)
                ? "en-US-JennyNeural"
                : DefaultVoice;
        }

        private static string BuildSsml(string text, string languageCode, string voiceName)
        {
            var escapedText = System.Security.SecurityElement.Escape(text) ?? string.Empty;
            return $"""
<?xml version="1.0" encoding="utf-8"?>
<speak version="1.0" xml:lang="{languageCode}" xmlns="http://www.w3.org/2001/10/synthesis" xmlns:mstts="http://www.w3.org/2001/mstts">
  <voice name="{voiceName}">
    <prosody rate="0%" pitch="0%">{escapedText}</prosody>
  </voice>
</speak>
""";
        }

        private static string NormalizeText(string text)
        {
            return Regex.Replace(text.Trim(), "\\s+", " ");
        }

        private string ToWebPath(string cacheFilePath)
        {
            var webRoot = _environment.WebRootPath ?? _environment.ContentRootPath;
            var relativePath = Path.GetRelativePath(webRoot, cacheFilePath).Replace(Path.DirectorySeparatorChar, '/');
            return $"/{relativePath}";
        }
    }
}