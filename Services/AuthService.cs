using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FamiHub.API.Data;
using FamiHub.API.DTOs;
using FamiHub.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace FamiHub.API.Services
{
    public class AuthService
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;

        public AuthService(AppDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        public async Task<AuthResponseDto?> RegisterAsync(RegisterDto dto)
        {
            if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
                return null;

            if (!Enum.TryParse<UserRole>(dto.Role, out var role))
                role = UserRole.Child;

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = role
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var token = GenerateToken(user);
            return new AuthResponseDto { Token = token, User = MapToDto(user) };
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
        {
            var user = await _db.Users
                .Include(u => u.Family)
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return null;

            var token = GenerateToken(user);
            return new AuthResponseDto { Token = token, User = MapToDto(user) };
        }

        private string GenerateToken(User user)
        {
            var jwtKey = _config["Jwt:Key"] ?? "FamiHubSecretKey_ChangeInProduction_2024";
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("FamilyId", user.FamilyId?.ToString() ?? ""),
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"] ?? "FamiHub",
                audience: _config["Jwt:Audience"] ?? "FamiHubApp",
                claims: claims,
                expires: DateTime.UtcNow.AddDays(30),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<UserDto?> GetUserByIdAsync(int userId)
        {
            var user = await _db.Users
                .Include(u => u.Family)
                .FirstOrDefaultAsync(u => u.Id == userId);
            return user == null ? null : MapToDto(user);
        }

        public static UserDto MapToDto(User user) => new()
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role.ToString(),
            FamilyId = user.FamilyId,
            FamilyName = user.Family?.Name,
            Points = user.Points,
            Avatar = user.Avatar
        };
    }
}
