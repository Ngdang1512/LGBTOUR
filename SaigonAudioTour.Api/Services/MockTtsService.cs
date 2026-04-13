using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SaigonAudioTour.Api.Services
{
    public class MockTtsService : ITtsService
    {
        private readonly IWebHostEnvironment _environment;

        public MockTtsService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        /// <summary>
        /// Mock TTS: Creates a simple WAV file from text
        /// For production, replace with actual TTS API (Google Cloud, Azure, etc.)
        /// </summary>
        public async Task<string?> TextToSpeechAsync(string text, string languageCode)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;

            try
            {
                var fileName = Guid.NewGuid().ToString() + ".wav";
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "audios");

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var filePath = Path.Combine(uploadsFolder, fileName);

                // Create a minimal valid WAV file
                // WAV header + simple sine wave tone
                var waveData = GenerateSimpleWave(text, languageCode);
                await File.WriteAllBytesAsync(filePath, waveData);

                return $"/audios/{fileName}";
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Generates a minimal valid WAV file
        /// </summary>
        private byte[] GenerateSimpleWave(string text, string languageCode)
        {
            const int sampleRate = 44100;
            const int bitsPerSample = 16;
            const int channels = 1;
            
            // Duration based on text length (rough estimate: 50ms per character)
            int durationMs = Math.Min(text.Length * 50, 5000);
            int sampleCount = (sampleRate * durationMs) / 1000;

            byte[] samples = new byte[sampleCount * (bitsPerSample / 8) * channels];

            // Generate simple tone (sine wave at 440Hz - A note)
            // This is just placeholder audio to simulate TTS
            double frequency = 440.0;
            double amplitude = 16000;

            for (int i = 0; i < sampleCount; i++)
            {
                double t = (double)i / sampleRate;
                double value = amplitude * Math.Sin(2 * Math.PI * frequency * t);
                short sample = (short)Math.Clamp(value, short.MinValue, short.MaxValue);

                int byteIndex = i * 2;
                samples[byteIndex] = (byte)(sample & 0xFF);
                samples[byteIndex + 1] = (byte)((sample >> 8) & 0xFF);
            }

            // WAV header
            byte[] header = CreateWavHeader(sampleRate, bitsPerSample, channels, samples.Length);
            byte[] wavFile = new byte[header.Length + samples.Length];

            Buffer.BlockCopy(header, 0, wavFile, 0, header.Length);
            Buffer.BlockCopy(samples, 0, wavFile, header.Length, samples.Length);

            return wavFile;
        }

        private byte[] CreateWavHeader(int sampleRate, int bitsPerSample, int channels, int dataSize)
        {
            byte[] header = new byte[44];

            // RIFF header
            header[0] = (byte)'R';
            header[1] = (byte)'I';
            header[2] = (byte)'F';
            header[3] = (byte)'F';

            // File size - 8
            int fileSize = dataSize + 36;
            BitConverter.GetBytes(fileSize).CopyTo(header, 4);

            // WAVE header
            header[8] = (byte)'W';
            header[9] = (byte)'A';
            header[10] = (byte)'V';
            header[11] = (byte)'E';

            // fmt subchunk
            header[12] = (byte)'f';
            header[13] = (byte)'m';
            header[14] = (byte)'t';
            header[15] = (byte)' ';

            // Subchunk1 size (16 for PCM)
            BitConverter.GetBytes(16).CopyTo(header, 16);

            // Audio format (1 for PCM)
            BitConverter.GetBytes((short)1).CopyTo(header, 20);

            // Number of channels
            BitConverter.GetBytes((short)channels).CopyTo(header, 22);

            // Sample rate
            BitConverter.GetBytes(sampleRate).CopyTo(header, 24);

            // Byte rate
            int byteRate = sampleRate * channels * (bitsPerSample / 8);
            BitConverter.GetBytes(byteRate).CopyTo(header, 28);

            // Block align
            BitConverter.GetBytes((short)(channels * (bitsPerSample / 8))).CopyTo(header, 32);

            // Bits per sample
            BitConverter.GetBytes((short)bitsPerSample).CopyTo(header, 34);

            // data subchunk
            header[36] = (byte)'d';
            header[37] = (byte)'a';
            header[38] = (byte)'t';
            header[39] = (byte)'a';

            // Subchunk2 size
            BitConverter.GetBytes(dataSize).CopyTo(header, 40);

            return header;
        }
    }
}
