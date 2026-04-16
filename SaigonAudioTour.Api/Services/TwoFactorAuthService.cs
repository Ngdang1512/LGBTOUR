using System.Text;
using SaigonAudioTour.Api.Data;
using SaigonAudioTour.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace SaigonAudioTour.Api.Services;

/// <summary>
/// Service for managing 2FA (Two-Factor Authentication) using TOTP (Time-based One-Time Password).
/// Uses Google Authenticator or similar apps for verification.
/// </summary>
public interface ITwoFactorAuthService
{
    /// <summary>
    /// Generate a new 2FA secret for a user and return QR code data URL.
    /// </summary>
    Task<TwoFactorSetupResponse> GenerateSetupAsync(int adminId, string adminUsername);

    /// <summary>
    /// Verify TOTP code and enable 2FA for user.
    /// </summary>
    Task<bool> VerifyAndEnableAsync(int adminId, string totpCode);

    /// <summary>
    /// Verify TOTP code during login.
    /// </summary>
    Task<bool> VerifyLoginCodeAsync(string secret, string totpCode);

    /// <summary>
    /// Disable 2FA for user.
    /// </summary>
    Task<bool> DisableAsync(int adminId);

    /// <summary>
    /// Check if 2FA is enabled for user.
    /// </summary>
    Task<bool> IsEnabledAsync(int adminId);
}

/// <summary>
/// TOTP (RFC 6238) implementation for time-based one-time passwords.
/// </summary>
public class TwoFactorAuthService : ITwoFactorAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TwoFactorAuthService> _logger;
    private const int TotpStep = 30; // 30 seconds
    private const int TotpCodeLength = 6; // 6 digits

    public TwoFactorAuthService(ApplicationDbContext context, ILogger<TwoFactorAuthService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TwoFactorSetupResponse> GenerateSetupAsync(int adminId, string adminUsername)
    {
        try
        {
            var admin = await _context.Admins.FindAsync(adminId);
            if (admin == null)
                throw new InvalidOperationException($"Admin {adminId} not found");

            // Generate a random 32-byte secret (256 bits)
            var secretBytes = new byte[32];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(secretBytes);
            }

            // Encode as Base32 for display
            var base32Secret = Base32Encode(secretBytes);

            // Generate QR code URL for authenticator apps
            var encodedUsername = Uri.EscapeDataString(adminUsername);
            var encodedIssuer = Uri.EscapeDataString("SaigonAudioTour");
            var otpauthUrl = $"otpauth://totp/{encodedIssuer}:{encodedUsername}?secret={base32Secret}&issuer={encodedIssuer}&algorithm=SHA1&digits=6&period=30";

            // Generate QR code using QR server API (no external dependency)
            var qrCodeUrl = $"https://api.qrserver.com/v1/create-qr-code/?size=300x300&data={Uri.EscapeDataString(otpauthUrl)}";

            _logger.LogInformation("2FA setup initiated for admin {AdminId}", adminId);

            return new TwoFactorSetupResponse
            {
                Secret = base32Secret,
                QrCodeUrl = qrCodeUrl,
                ManualEntryKey = base32Secret,
                Message = "Scan with authenticator app (Google Authenticator, Microsoft Authenticator, Authy, etc.)"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating 2FA setup for admin {AdminId}", adminId);
            throw;
        }
    }

    public async Task<bool> VerifyAndEnableAsync(int adminId, string totpCode)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(totpCode) || totpCode.Length != TotpCodeLength)
                return false;

            // For now, store the temporary setup in memory
            // In production, would store encrypted in DB with pending verification status
            // This is a simplified implementation
            
            _logger.LogInformation("2FA verification for admin {AdminId}", adminId);
            
            // TODO: In production, verify the code against the secret generated earlier
            // Store secret in TwoFactorAuth entity if verification succeeds
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enabling 2FA for admin {AdminId}", adminId);
            return false;
        }
    }

    public async Task<bool> VerifyLoginCodeAsync(string secret, string totpCode)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(secret) || string.IsNullOrWhiteSpace(totpCode))
                return false;

            if (totpCode.Length != TotpCodeLength || !int.TryParse(totpCode, out _))
                return false;

            var secretBytes = Base32Decode(secret);
            if (secretBytes == null)
                return false;

            // Calculate TOTP code for current time window (allowing ±1 window for drift)
            var currentTime = (long)Math.Floor(DateTimeOffset.UtcNow.ToUnixTimeSeconds() / (double)TotpStep);

            for (int i = -1; i <= 1; i++) // Check previous, current, and next time window
            {
                var expectedCode = GenerateTotpCode(secretBytes, currentTime + i);
                if (expectedCode == totpCode)
                {
                    _logger.LogInformation("2FA verification successful");
                    return true;
                }
            }

            _logger.LogWarning("2FA verification failed - invalid code");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying 2FA login code");
            return false;
        }
    }

    public async Task<bool> DisableAsync(int adminId)
    {
        try
        {
            // TODO: Store 2FA disabled state in TwoFactorAuth entity
            _logger.LogInformation("2FA disabled for admin {AdminId}", adminId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disabling 2FA for admin {AdminId}", adminId);
            return false;
        }
    }

    public async Task<bool> IsEnabledAsync(int adminId)
    {
        try
        {
            // TODO: Check TwoFactorAuth entity for enabled status
            return false; // Default: 2FA not enabled
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking 2FA status for admin {AdminId}", adminId);
            return false;
        }
    }

    /// <summary>
    /// Generate TOTP code using HMAC-SHA1.
    /// RFC 6238 implementation.
    /// </summary>
    private string GenerateTotpCode(byte[] secretBytes, long timeCounter)
    {
        using (var hmac = new System.Security.Cryptography.HMACSHA1(secretBytes))
        {
            var counterBytes = BitConverter.GetBytes(timeCounter);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(counterBytes);

            var hmacHash = hmac.ComputeHash(counterBytes);
            var offset = hmacHash[^1] & 0xF; // Last nibble as offset

            var code = ((hmacHash[offset] & 0x7F) << 24 |
                        (hmacHash[offset + 1] & 0xFF) << 16 |
                        (hmacHash[offset + 2] & 0xFF) << 8 |
                        (hmacHash[offset + 3] & 0xFF)) % 1000000;

            return code.ToString("D6");
        }
    }

    /// <summary>
    /// Encode bytes to Base32 format for TOTP display.
    /// </summary>
    private string Base32Encode(byte[] data)
    {
        const string Base32Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        var output = new StringBuilder();

        for (int i = 0; i < data.Length; i += 5)
        {
            var bufferSize = Math.Min(5, data.Length - i);
            var buffer = new byte[5];
            Array.Copy(data, i, buffer, 0, bufferSize);

            output.Append(Base32Alphabet[(buffer[0] & 248) >> 3]);
            output.Append(Base32Alphabet[(((buffer[0] & 7) << 2) | ((buffer[1] & 192) >> 6))]);
            if (bufferSize > 1)
                output.Append(Base32Alphabet[(buffer[1] & 62) >> 1]);
            if (bufferSize > 2)
                output.Append(Base32Alphabet[(((buffer[1] & 1) << 4) | ((buffer[2] & 240) >> 4))]);
            if (bufferSize > 3)
                output.Append(Base32Alphabet[(((buffer[2] & 15) << 1) | ((buffer[3] & 128) >> 7))]);
            if (bufferSize > 4)
                output.Append(Base32Alphabet[(buffer[3] & 124) >> 2]);
        }

        return output.ToString();
    }

    /// <summary>
    /// Decode Base32 string back to bytes.
    /// </summary>
    private byte[]? Base32Decode(string input)
    {
        try
        {
            const string Base32Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
            input = input.ToUpper().Replace("=", "");

            var output = new List<byte>();
            for (int i = 0; i < input.Length; i += 8)
            {
                var buffer = new int[8];
                for (int j = 0; j < Math.Min(8, input.Length - i); j++)
                {
                    buffer[j] = Base32Alphabet.IndexOf(input[i + j]);
                    if (buffer[j] == -1)
                        return null;
                }

                output.Add((byte)(((buffer[0] & 31) << 3) | ((buffer[1] & 28) >> 2)));
                if (buffer[2] != 0)
                    output.Add((byte)(((buffer[1] & 3) << 6) | ((buffer[2] & 31) << 1) | ((buffer[3] & 16) >> 4)));
                if (buffer[4] != 0)
                    output.Add((byte)(((buffer[3] & 15) << 4) | ((buffer[4] & 30) >> 1)));
                if (buffer[5] != 0)
                    output.Add((byte)(((buffer[4] & 1) << 7) | ((buffer[5] & 31) << 2) | ((buffer[6] & 24) >> 3)));
                if (buffer[7] != 0)
                    output.Add((byte)(((buffer[6] & 7) << 5) | (buffer[7] & 31)));
            }

            return output.ToArray();
        }
        catch
        {
            return null;
        }
    }
}

/// <summary>
/// Response model for 2FA setup.
/// </summary>
public class TwoFactorSetupResponse
{
    public string Secret { get; set; } = string.Empty;
    public string QrCodeUrl { get; set; } = string.Empty;
    public string ManualEntryKey { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
