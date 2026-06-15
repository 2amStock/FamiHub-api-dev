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
        private readonly IEmailService _emailService;

        public AuthService(AppDbContext db, IConfiguration config, IEmailService emailService)
        {
            _db = db;
            _config = config;
            _emailService = emailService;
        }

        public async Task<AuthResponseDto?> RegisterAsync(RegisterDto dto)
        {
            if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
                return null; // Handle this in controller, maybe return a specific result

            if (!Enum.TryParse<UserRole>(dto.Role, out var role))
                role = UserRole.Child;

            if (role == UserRole.Parent)
            {
                var otp = new Random().Next(100000, 999999).ToString();
                
                var user = new User
                {
                    Name = dto.Name,
                    Email = dto.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    Role = role,
                    IsEmailVerified = false,
                    OtpCode = otp,
                    OtpExpiryTime = DateTime.UtcNow.AddMinutes(5)
                };

                _db.Users.Add(user);
                await _db.SaveChangesAsync();

                var subject = "Mã xác minh tài khoản FamiHub";
                var htmlBody = $@"
                    <div style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; border: 1px solid #eaeaea; padding: 24px; border-radius: 12px;'>
                        <div style='text-align: center; margin-bottom: 24px;'>
                            <h2 style='color: #FF7EB3; margin: 0;'>FamiHub</h2>
                        </div>
                        <p>Chào bạn,</p>
                        <p>Chúng tôi nhận được yêu cầu xác thực email cho tài khoản FamiHub của bạn. Dưới đây là mã xác minh (OTP):</p>
                        <div style='text-align: center; margin: 32px 0;'>
                            <span style='font-size: 32px; font-weight: bold; letter-spacing: 6px; color: #4A90E2; background: #f8f9fa; padding: 16px 32px; border-radius: 8px; border: 1px solid #e2e8f0;'>{otp}</span>
                        </div>
                        <p>Mã xác minh này có hiệu lực trong vòng <b>5 phút</b>.</p>
                        <p style='color: #718096; font-size: 13px; margin-top: 32px; border-top: 1px solid #eaeaea; padding-top: 16px;'>
                            Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email. Hệ thống sẽ tự động hủy yêu cầu.<br>
                            Mọi thắc mắc vui lòng liên hệ: <a href='mailto:support@famihub.com' style='color: #4A90E2;'>support@famihub.com</a>.
                        </p>
                    </div>";

                await _emailService.SendEmailAsync(user.Email, subject, htmlBody, isHtml: true);

                // Do not generate token yet
                return new AuthResponseDto { Token = string.Empty, User = MapToDto(user) };
            }
            else
            {
                var user = new User
                {
                    Name = dto.Name,
                    Email = dto.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    Role = role,
                    IsEmailVerified = true,
                };

                _db.Users.Add(user);
                await _db.SaveChangesAsync();

                var token = GenerateToken(user);
                return new AuthResponseDto { Token = token, User = MapToDto(user) };
            }
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
        {
            var user = await _db.Users
                .Include(u => u.Family)
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return null;

            if (!user.IsEmailVerified)
                throw new Exception("unverified_email");

            var token = GenerateToken(user);
            return new AuthResponseDto { Token = token, User = MapToDto(user) };
        }

        public async Task<bool> VerifyOtpAsync(VerifyOtpDto dto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null) return false;

            if (user.IsEmailVerified) return true;

            if (user.OtpCode == dto.OtpCode && user.OtpExpiryTime > DateTime.UtcNow)
            {
                user.IsEmailVerified = true;
                user.OtpCode = null;
                user.OtpExpiryTime = null;
                await _db.SaveChangesAsync();
                return true;
            }
            
            return false;
        }

        public async Task<bool> ResendOtpAsync(ResendOtpDto dto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null || user.IsEmailVerified) return false;

            var otp = new Random().Next(100000, 999999).ToString();
            user.OtpCode = otp;
            user.OtpExpiryTime = DateTime.UtcNow.AddMinutes(5);
            await _db.SaveChangesAsync();

            var subject = "Mã xác minh tài khoản FamiHub (Gửi lại)";
            var htmlBody = $@"
                <div style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; border: 1px solid #eaeaea; padding: 24px; border-radius: 12px;'>
                    <div style='text-align: center; margin-bottom: 24px;'>
                        <h2 style='color: #FF7EB3; margin: 0;'>FamiHub</h2>
                    </div>
                    <p>Chào bạn,</p>
                    <p>Bạn vừa yêu cầu gửi lại mã xác thực email. Dưới đây là mã xác minh (OTP) mới của bạn:</p>
                    <div style='text-align: center; margin: 32px 0;'>
                        <span style='font-size: 32px; font-weight: bold; letter-spacing: 6px; color: #4A90E2; background: #f8f9fa; padding: 16px 32px; border-radius: 8px; border: 1px solid #e2e8f0;'>{otp}</span>
                    </div>
                    <p>Mã xác minh này có hiệu lực trong vòng <b>5 phút</b>.</p>
                    <p style='color: #718096; font-size: 13px; margin-top: 32px; border-top: 1px solid #eaeaea; padding-top: 16px;'>
                        Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email. Hệ thống sẽ tự động hủy yêu cầu.<br>
                        Mọi thắc mắc vui lòng liên hệ: <a href='mailto:support@famihub.com' style='color: #4A90E2;'>support@famihub.com</a>.
                    </p>
                </div>";

            await _emailService.SendEmailAsync(user.Email, subject, htmlBody, isHtml: true);
            
            return true;
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
            Avatar = user.Avatar,
            CurrentPlanId = user.CurrentPlanId,
            SubscriptionExpiryTime = user.SubscriptionExpiryTime
        };
    }
}
