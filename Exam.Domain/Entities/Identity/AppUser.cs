using Exam.Domain.Enum;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace Exam.Domain.Entities.Identity
{
    public class AppUser : IdentityUser<int>
    {
        public string FirstName { get; set; } = string.Empty;
        // الاسم الأول للمستخدم (Student / Instructor / Admin)

        public string LastName { get; set; } = string.Empty;
        // اسم العائلة لعرض الاسم الكامل في النظام

        public DateTime DateOfBirth { get; set; }
        // تاريخ الميلاد (اختياري حسب متطلبات النظام الأكاديمي)

        public string Address { get; set; } = string.Empty;
        // العنوان كنص مباشر
        // حذفنا AddressID لأن مفيش جدول Address في المشروع

        public Gender Gender { get; set; }
        // Enum للنوع بدل string
        // يمنع القيم العشوائية ويحافظ على Data Integrity

        public bool IsDeleted { get; set; }
        // Soft Delete لتعطيل الحساب بدون حذفه
        // مهم في أنظمة الامتحانات للحفاظ على النتائج والسجلات
        public bool IsActive { get; set; } = true;
        // لتعطيل الحساب في حالة الغش أو المخالفات

        public string? Embeddings { get; set; }
        // بيانات Face Recognition / AI (لو مستخدمة لمنع الغش)

        public string? ImageURL { get; set; }
        // صورة المستخدم (مفيدة في الـ Proctoring أو البروفايل)

        public string FullName => $"{FirstName} {LastName}";
        // Property محسوبة لعرض الاسم الكامل بدون تخزينه في قاعدة البيانات

        public UserType UserType { get; set; }
        // نوع المستخدم (Student, Instructor, Admin)

        public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        // Refresh Tokens لنظام JWT Authentication
        // تسمح بتجديد الجلسة بشكل آمن بدون إعادة تسجيل الدخول
    }
}
