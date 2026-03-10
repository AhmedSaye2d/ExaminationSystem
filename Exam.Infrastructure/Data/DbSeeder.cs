using Exam.Domain.Entities;
using Exam.Domain.Entities.Identity;
using Exam.Domain.Enum;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Exam.Infrastructure.Data
{
    public static class DbSeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();

            // Seed Roles
            var roles = Enum.GetNames(typeof(UserType));
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole<int>(role));
                }
            }

            // Seed Admin User
            var adminEmail = "admin@exam.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new AppUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "System",
                    LastName = "Admin",
                    UserType = UserType.Admin,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, UserType.Admin.ToString());
                }
            }
        }

        public static async Task SeedDataAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<AppDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();

            // 1. Departments
            if (!await context.Departments.AnyAsync())
            {
                var depts = new List<Department>
                {
                    new Department { Name = "Computer Science", Description = "Topics regarding computer science, software, algorithms, data structures" },
                    new Department { Name = "Software Engineering", Description = "System design, patterns, requirements, testing" }
                };
                await context.Departments.AddRangeAsync(depts);
                await context.SaveChangesAsync();
            }

            var csDept = await context.Departments.FirstOrDefaultAsync(d => d.Name == "Computer Science");
            var seDept = await context.Departments.FirstOrDefaultAsync(d => d.Name == "Software Engineering");
            
            if (csDept == null || seDept == null) return;

            // 2. Instructors & Students
            if (!await context.Instructors.AnyAsync())
            {
                var inst1 = new Instructor { UserName = "sara@exam.com", Email = "sara@exam.com", FirstName = "Sara", LastName = "Omar", UserType = UserType.Instructor, DepartmentId = csDept.Id, EmailConfirmed = true, HireDate = DateTime.UtcNow.AddYears(-2) };
                var inst2 = new Instructor { UserName = "ahmed@exam.com", Email = "ahmed@exam.com", FirstName = "Ahmed", LastName = "Ali", UserType = UserType.Instructor, DepartmentId = seDept.Id, EmailConfirmed = true, HireDate = DateTime.UtcNow.AddYears(-5) };
                
                await userManager.CreateAsync(inst1, "P@ssword123");
                await userManager.AddToRoleAsync(inst1, UserType.Instructor.ToString());
                
                await userManager.CreateAsync(inst2, "P@ssword123");
                await userManager.AddToRoleAsync(inst2, UserType.Instructor.ToString());
            }

            if (!await context.Students.AnyAsync())
            {
                var stu1 = new Student { UserName = "student1@example.com", Email = "student1@example.com", FirstName = "Youssef", LastName = "Nabil", UserType = UserType.Student, MajorId = csDept.Id, EmailConfirmed = true, Gender = Gender.Male, GPA = 3.6 };
                var stu2 = new Student { UserName = "student2@example.com", Email = "student2@example.com", FirstName = "Nour", LastName = "Hassan", UserType = UserType.Student, MajorId = seDept.Id, EmailConfirmed = true, Gender = Gender.Female, GPA = 3.8 };
                var stu3 = new Student { UserName = "postman@example.com", Email = "postman@example.com", FirstName = "Postman", LastName = "Tester", UserType = UserType.Student, MajorId = csDept.Id, EmailConfirmed = true, Gender = Gender.Male, GPA = 4.0 };
                
                await userManager.CreateAsync(stu1, "P@ssword123");
                await userManager.AddToRoleAsync(stu1, UserType.Student.ToString());
                
                await userManager.CreateAsync(stu2, "P@ssword123");
                await userManager.AddToRoleAsync(stu2, UserType.Student.ToString());

                await userManager.CreateAsync(stu3, "P@ssword123");
                await userManager.AddToRoleAsync(stu3, UserType.Student.ToString());
            }

            var instructor1 = await context.Instructors.FirstOrDefaultAsync(i => i.Email == "sara@exam.com");
            var student1 = await context.Students.FirstOrDefaultAsync(s => s.Email == "postman@example.com");

            if (instructor1 == null || student1 == null) return;

            // 3. Courses
            if (!await context.Courses.AnyAsync())
            {
                var courses = new List<Course>
                {
                    new Course { Name = "Data Structures", Code = "CS201", Description = "Learn basic Data Structures", DepartmentId = csDept.Id, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddMonths(3), CreditHours = 3 },
                    new Course { Name = "Algorithms", Code = "CS301", Description = "Design Patterns and Algorithms", DepartmentId = csDept.Id, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddMonths(3), CreditHours = 3 },
                    new Course { Name = "Software Process", Code = "SE201", Description = "Software Engineering Life Cycle", DepartmentId = seDept.Id, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddMonths(3), CreditHours = 3 }
                };
                await context.Courses.AddRangeAsync(courses);
                await context.SaveChangesAsync();

                // Assign Instructor and Students to Course
                var c1 = await context.Courses.FirstOrDefaultAsync(c => c.Code == "CS201");
                if (c1 != null)
                {
                    context.CourseInstructors.Add(new CourseInstructor { CourseId = c1.Id, InstructorId = instructor1.Id });
                    context.CourseStudents.Add(new CourseStudent { CourseId = c1.Id, StudentId = student1.Id });
                    await context.SaveChangesAsync();
                }
            }

            var course1 = await context.Courses.FirstOrDefaultAsync(c => c.Code == "CS201");
            if (course1 == null) return;

            // 4. Questions & Choices
            if (!await context.Questions.AnyAsync())
            {
                var q1 = new Question { Text = "Which data structure uses LIFO?", Type = QuestionType.MCQ, DifficultyLevel = 1, Grade = 2 };
                q1.Choices = new HashSet<Choice>
                {
                    new Choice { Text = "Queue", IsCorrectAnswer = false, Order = 1 },
                    new Choice { Text = "Stack", IsCorrectAnswer = true, Order = 2 },
                    new Choice { Text = "Linked List", IsCorrectAnswer = false, Order = 3 }
                };

                var q2 = new Question { Text = "Which data structure uses FIFO?", Type = QuestionType.MCQ, DifficultyLevel = 1, Grade = 2 };
                q2.Choices = new HashSet<Choice>
                {
                    new Choice { Text = "Stack", IsCorrectAnswer = false, Order = 1 },
                    new Choice { Text = "Queue", IsCorrectAnswer = true, Order = 2 },
                    new Choice { Text = "Array", IsCorrectAnswer = false, Order = 3 }
                };

                var q3 = new Question { Text = "Array elements are stored in contiguous memory locations.", Type = QuestionType.TrueFalse, DifficultyLevel = 1, Grade = 1 };
                q3.Choices = new HashSet<Choice>
                {
                    new Choice { Text = "True", IsCorrectAnswer = true, Order = 1 },
                    new Choice { Text = "False", IsCorrectAnswer = false, Order = 2 }
                };

                await context.Questions.AddRangeAsync(q1, q2, q3);
                await context.SaveChangesAsync();
            }

            // 5. Exams & Settings
            if (!await context.Exams.AnyAsync())
            {
                var exam = new Exam.Domain.Entities.Exam
                {
                    Name = "Data Structures Midterm",
                    Description = "Covers Arrays, Stacks, and Queues",
                    StartDate = DateTime.UtcNow.AddDays(-1), // Open from yesterday
                    DueDate = DateTime.UtcNow.AddDays(7), // Available for a week
                    TotalQuestions = 3,
                    TotalPoints = 5,
                    Type = ExamType.Midterm,
                    CourseID = course1.Id,
                    InstructorID = instructor1.Id,
                    Settings = new ExamSettings 
                    { 
                        DurationMinutes = 60, 
                        ShuffleQuestions = true, 
                        ShuffleChoices = true, 
                        ShowResultAfterSubmit = true, 
                        AllowReview = true 
                    }
                };
                await context.Exams.AddAsync(exam);
                await context.SaveChangesAsync();

                var questions = await context.Questions.Take(3).ToListAsync();
                foreach (var (q, index) in questions.Select((q, i) => (q, i)))
                {
                    context.ExamQuestions.Add(new ExamQuestion { ExamId = exam.Id, QuestionId = q.Id, Points = q.Grade });
                }
                
                // Set total points
                exam.TotalPoints = questions.Sum(q => q.Grade);
                await context.SaveChangesAsync();
            }
        }
    }
}
