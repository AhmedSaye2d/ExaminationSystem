using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Exam.Domain.Interface.Authentication
{
    public interface ITokenManagement
    {
        // إنشاء JWT Token جديد
        string GenerateToken(List<Claim> claims);

        // إنشاء Refresh Token عشوائي
        string GetRefreshToken();

        // استخراج Claims من JWT Token
        List<Claim> GetUserClaims(string token);

        // التحقق من صلاحية Refresh Token
        Task<bool> ValidateRefreshToken(string refreshToken);

        // الحصول على UserId بناءً على Refresh Token
        Task<int?> GetUserIdByRefreshToken(string refreshToken);

        // إضافة Refresh Token جديد للمستخدم
        Task<int> AddRefreshToken(int userId, string refreshToken);

        // تحديث Refresh Token لمستخدم موجود
        Task<int> UpdateRefreshToken(int userId, string refreshToken);
        Task<bool> ValidateRefreshTokenForUser(int userId);
        Task<bool> RevokeRefreshToken(string refreshToken);

    }
}

