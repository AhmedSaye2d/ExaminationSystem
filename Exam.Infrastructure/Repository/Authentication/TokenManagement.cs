using Exam.Domain.Entities.Identity;
using Exam.Domain.Interface.Authentication;
using Exam.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

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

        // ================= JWT =================
        public string GenerateToken(List<Claim> claims)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!)
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // ================= Refresh Token =================
        public string GetRefreshToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }

        public async Task<int> AddRefreshToken(int userId, string refreshToken)
        {
            var token = new RefreshToken
            {
                UserId = userId,
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };

            _context.RefreshTokens.Add(token);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> UpdateRefreshToken(int userId, string refreshToken)
        {
            var token = await _context.RefreshTokens
                .FirstOrDefaultAsync(x => x.UserId == userId && !x.IsRevoked);

            if (token == null)
                return 0;

            token.Token = refreshToken;
            token.ExpiresAt = DateTime.UtcNow.AddDays(7);

            return await _context.SaveChangesAsync();
        }

        public async Task<bool> ValidateRefreshToken(string refreshToken)
        {
            return await _context.RefreshTokens.AnyAsync(x =>
                x.Token == refreshToken &&
                !x.IsRevoked &&
                x.ExpiresAt > DateTime.UtcNow);
        }

        public async Task<int?> GetUserIdByRefreshToken(string refreshToken)
        {
            var token = await _context.RefreshTokens.FirstOrDefaultAsync(x =>
                x.Token == refreshToken &&
                !x.IsRevoked &&
                x.ExpiresAt > DateTime.UtcNow);

            return token?.UserId;
        }

        public async Task<bool> ValidateRefreshTokenForUser(int userId)
        {
            return await _context.RefreshTokens.AnyAsync(x =>
                x.UserId == userId &&
                !x.IsRevoked &&
                x.ExpiresAt > DateTime.UtcNow);
        }

        public List<Claim> GetUserClaims(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            return jwt.Claims.ToList();
        }
        public async Task<bool> RevokeRefreshToken(string refreshToken)
        {
            var token = await _context.RefreshTokens
                .FirstOrDefaultAsync(x => x.Token == refreshToken && !x.IsRevoked);

            if (token == null)
                return false;

            token.IsRevoked = true;
            await _context.SaveChangesAsync();
            return true;
        }
    }

}

