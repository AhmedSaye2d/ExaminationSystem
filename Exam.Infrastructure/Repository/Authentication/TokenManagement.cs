using Exam.Domain.Entities.Identity;
using Exam.Domain.Interface.Authentication;
using Exam.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Exam.Infrastructure.Repository.Authentication
{
    public class TokenManagement : ITokenManagement
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public TokenManagement(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // ✅ توليد JWT Token
        public string GenerateToken(List<Claim> claims)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiration = DateTime.UtcNow.AddHours(2);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: expiration,
                signingCredentials: cred
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // ✅ إنشاء Refresh Token عشوائي وآمن (بدون ترميز)
        public string GetRefreshToken()
        {
            const int Bytes = 64;
            byte[] bytes = new byte[Bytes];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }

            // ❌ لا تستخدم WebUtility.UrlEncode هنا
            return Convert.ToBase64String(bytes);
        }

        // ✅ استخراج Claims من JWT Token
        public List<Claim> GetUserClaims(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            return jwtToken?.Claims?.ToList() ?? new List<Claim>();
        }

        // ✅ الحصول على UserId من RefreshToken
        public async Task<string?> GetUserIdByRefreshToken(string refreshToken)
        {
            var token = await _context.RefreshToken
                .FirstOrDefaultAsync(e => e.Token == refreshToken);
            return token?.UserId;
        }

        // ✅ إضافة Refresh Token جديد
        public async Task<int> AddRefreshToken(string userId, string refreshToken)
        {
            _context.RefreshToken.Add(new RefreshToken
            {
                UserId = userId,
                Token = refreshToken
            });

            return await _context.SaveChangesAsync();
        }

        // ✅ تحديث Refresh Token موجود
        public async Task<int> UpdateRefreshToken(string userId, string refreshToken)
        {
            var existingToken = await _context.RefreshToken
                .FirstOrDefaultAsync(e => e.UserId == userId);

            if (existingToken == null)
                return -1;

            existingToken.Token = refreshToken;
            return await _context.SaveChangesAsync();
        }

        // ✅ التحقق من صحة الـ Refresh Token
        public async Task<bool> ValidateRefreshToken(string refreshToken)
        {
            var user = await _context.RefreshToken
                .FirstOrDefaultAsync(e => e.Token == refreshToken);
            return user != null;
        }

        // ✅ التحقق هل المستخدم عنده توكن أساساً
        public async Task<bool> ValidateRefreshTokenForUser(string userId)
        {
            return await _context.RefreshToken.AnyAsync(r => r.UserId == userId);
        }
    }
}
