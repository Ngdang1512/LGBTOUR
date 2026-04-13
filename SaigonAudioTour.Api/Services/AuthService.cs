using SaigonAudioTour.Api.Data;
using SaigonAudioTour.Api.DTOs.Auth;
using SaigonAudioTour.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace SaigonAudioTour.Api.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;

        public AuthService(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task<AuthResultDto?> LoginAsync(LoginDto dto)
        {
            var email = (dto.Username ?? string.Empty).Trim().ToLowerInvariant();
            var user = await _context.Users.FirstOrDefaultAsync(a => a.Email == email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return null;

            var token = CreateJwt(user.Email, "User");

            return new AuthResultDto
            {
                Token = token,
                UserId = user.Id,
                Email = user.Email,
                FullName = string.IsNullOrWhiteSpace(user.FullName) ? user.Email : user.FullName
            };
        }

        public async Task<AuthResultDto?> RegisterAsync(RegisterDto dto)
        {
            var email = (dto.Email ?? string.Empty).Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(email)) return null;

            var existed = await _context.Users.AnyAsync(a => a.Email == email);
            if (existed) return null;

            var user = new AppUser
            {
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                FullName = dto.FullName.Trim()
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = CreateJwt(user.Email, "User");

            return new AuthResultDto
            {
                Token = token,
                UserId = user.Id,
                Email = user.Email,
                FullName = string.IsNullOrWhiteSpace(user.FullName) ? user.Email : user.FullName
            };
        }

        public async Task<AuthResultDto?> AdminLoginAsync(LoginDto dto)
        {
            var username = (dto.Username ?? string.Empty).Trim();
            var admin = await _context.Admins.AsNoTracking().FirstOrDefaultAsync(a => a.Username == username);

            if (admin == null || !BCrypt.Net.BCrypt.Verify(dto.Password, admin.PasswordHash))
                return null;

            var token = CreateJwt(admin.Username, "Admin");

            return new AuthResultDto
            {
                Token = token,
                UserId = admin.Id,
                Email = admin.Username,
                FullName = string.IsNullOrWhiteSpace(admin.FullName) ? admin.Username : admin.FullName
            };
        }

        public async Task<AuthResultDto?> GetProfileByEmailAsync(string email)
        {
            var normalized = (email ?? string.Empty).Trim().ToLowerInvariant();
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(a => a.Email == normalized);
            if (user == null) return null;

            return new AuthResultDto
            {
                Token = string.Empty,
                UserId = user.Id,
                Email = user.Email,
                FullName = string.IsNullOrWhiteSpace(user.FullName) ? user.Email : user.FullName
            };
        }

        private string CreateJwt(string username, string role)
        {
            var jwtSettings = _config.GetSection("JwtSettings");
            var keyBytes = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Role, role)
                }),
                Expires = DateTime.UtcNow.AddHours(4),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256Signature),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
        }
    }
}