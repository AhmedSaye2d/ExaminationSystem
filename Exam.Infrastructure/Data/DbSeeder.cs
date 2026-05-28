using Exam.Domain.Constants;
using Exam.Domain.Entities;
using Exam.Domain.Entities.Identity;
using Exam.Domain.Enum;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Exam.Infrastructure.Data
{
    public static class DbSeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();

            var roles = new[] { AppRoles.Admin, AppRoles.Student, AppRoles.Instructor };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole<int>(role));
            }

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
                    await userManager.AddToRoleAsync(adminUser, AppRoles.Admin);
            }
            else
            {
                var userRoles = await userManager.GetRolesAsync(adminUser);
                if (!userRoles.Contains(AppRoles.Admin))
                    await userManager.AddToRoleAsync(adminUser, AppRoles.Admin);

                adminUser.UserType = UserType.Admin;
                await userManager.UpdateAsync(adminUser);
            }
        }

        public static async Task SeedDataAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<AppDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();

            // === MCQ TEST EXAM (runs first, before any early returns) ===
            await SeedMcqTestExamAsync(context);

            // === SCORING TEST EXAM (New for specific score testing) ===
            await SeedScoringTestExamAsync(context);

            // === COMPREHENSIVE SCORE TEST (10 Questions) ===
            await SeedComprehensiveExamAsync(context);

            // 1. Departments
            if (!await context.Departments.AnyAsync())
            {
                var depts = new List<Department>
                {
                    new Department { Name = "Computer Science", Description = "Algorithms, data structures, software" },
                    new Department { Name = "Software Engineering", Description = "System design, patterns, testing" }
                };
                await context.Departments.AddRangeAsync(depts);
                await context.SaveChangesAsync();
            }

            var csDept = await context.Departments.FirstOrDefaultAsync(d => d.Name == "Computer Science");
            var seDept = await context.Departments.FirstOrDefaultAsync(d => d.Name == "Software Engineering");

            if (csDept == null || seDept == null) return;

            // 2. Instructors
            if (!await context.Instructors.AnyAsync())
            {
                var inst1 = new Instructor { UserName = "sara@exam.com", Email = "sara@exam.com", FirstName = "Sara", LastName = "Omar", UserType = UserType.Instructor, DepartmentId = csDept.Id, EmailConfirmed = true, HireDate = DateTime.UtcNow.AddYears(-2) };
                var inst2 = new Instructor { UserName = "ahmed@exam.com", Email = "ahmed@exam.com", FirstName = "Ahmed", LastName = "Ali", UserType = UserType.Instructor, DepartmentId = seDept.Id, EmailConfirmed = true, HireDate = DateTime.UtcNow.AddYears(-5) };

                await userManager.CreateAsync(inst1, "P@ssword123");
                await userManager.AddToRoleAsync(inst1, AppRoles.Instructor);

                await userManager.CreateAsync(inst2, "P@ssword123");
                await userManager.AddToRoleAsync(inst2, AppRoles.Instructor);
            }

            // 3. Students
            if (!await context.Students.AnyAsync())
            {
                var stu1 = new Student { UserName = "student1@example.com", Email = "student1@example.com", FirstName = "Youssef", LastName = "Nabil", UserType = UserType.Student, MajorId = csDept.Id, EmailConfirmed = true, Gender = Gender.Male };
                var stu2 = new Student { UserName = "student2@example.com", Email = "student2@example.com", FirstName = "Nour", LastName = "Hassan", UserType = UserType.Student, MajorId = seDept.Id, EmailConfirmed = true, Gender = Gender.Female };
                var stu3 = new Student { UserName = "postman@example.com", Email = "postman@example.com", FirstName = "Postman", LastName = "Tester", UserType = UserType.Student, MajorId = csDept.Id, EmailConfirmed = true, Gender = Gender.Male };

                await userManager.CreateAsync(stu1, "P@ssword123");
                await userManager.AddToRoleAsync(stu1, AppRoles.Student);

                await userManager.CreateAsync(stu2, "P@ssword123");
                await userManager.AddToRoleAsync(stu2, AppRoles.Student);

                await userManager.CreateAsync(stu3, "P@ssword123");
                await userManager.AddToRoleAsync(stu3, AppRoles.Student);
            }

            var instructor1 = await context.Instructors.FirstOrDefaultAsync(i => i.Email == "sara@exam.com");
            var postmanStudent = await context.Students.FirstOrDefaultAsync(s => s.Email == "postman@example.com");
            var student1User = await context.Students.FirstOrDefaultAsync(s => s.Email == "student1@example.com");

            // 4. Courses
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

                var c1 = await context.Courses.FirstOrDefaultAsync(c => c.Code == "CS201");
                if (c1 != null && instructor1 != null && postmanStudent != null)
                {
                    context.CourseInstructors.Add(new CourseInstructor { CourseId = c1.Id, InstructorId = instructor1.Id });
                    context.CourseStudents.Add(new CourseStudent { CourseId = c1.Id, StudentId = postmanStudent.Id });
                    if (student1User != null)
                        context.CourseStudents.Add(new CourseStudent { CourseId = c1.Id, StudentId = student1User.Id });
                    await context.SaveChangesAsync();
                }
            }

            // Ensure all students are enrolled in CS201
            var course1 = await context.Courses.FirstOrDefaultAsync(c => c.Code == "CS201");
            if (course1 != null)
            {
                var allStudents = await context.Students.ToListAsync();
                var existingEnrollments = await context.CourseStudents
                    .Where(cs => cs.CourseId == course1.Id)
                    .Select(cs => cs.StudentId)
                    .ToListAsync();

                foreach (var stu in allStudents)
                {
                    if (!existingEnrollments.Contains(stu.Id))
                        context.CourseStudents.Add(new CourseStudent { CourseId = course1.Id, StudentId = stu.Id });
                }
                await context.SaveChangesAsync();
            }

            // Always ensure all exams have valid dates and are published
            var allExams = await context.Exams.ToListAsync();
            foreach (var e in allExams)
            {
                e.StartDate = DateTime.UtcNow.AddDays(-1);
                e.DueDate = DateTime.UtcNow.AddDays(365); // Open for a long time for testing
                e.IsPublished = true;
            }
            if (allExams.Any())
                await context.SaveChangesAsync();
        }

        private static async Task SeedMcqTestExamAsync(AppDbContext context)
        {
            if (await context.Exams.AnyAsync(e => e.Name == "MCQ Test Exam"))
                return;

            var course = await context.Courses.FirstOrDefaultAsync();
            var instructor = await context.Instructors.FirstOrDefaultAsync();

            if (course == null || instructor == null)
                return;

            var exam = new Exam.Domain.Entities.Exam
            {
                Name = "MCQ Test Exam",
                Description = "5 MCQ questions, 10 pts each. Correct answer = choice A.",
                StartDate = DateTime.UtcNow.AddDays(-1),
                DueDate = DateTime.UtcNow.AddDays(365),
                TotalQuestions = 5,
                TotalGrade = 50,
                Type = ExamType.Midterm,
                CourseID = course.Id,
                InstructorID = instructor.Id,
                IsPublished = true,
                PassingScore = 30,
                Settings = new ExamSettings
                {
                    DurationMinutes = 120,
                    ShuffleQuestions = false,
                    ShuffleChoices = false,
                    ShowResultAfterSubmit = true,
                    AllowReview = true,
                    MaxAttempts = 0
                }
            };
            await context.Exams.AddAsync(exam);
            await context.SaveChangesAsync();

            var questions = new List<Question>
            {
                new Question { Text = "What is the main language of .NET?", Type = QuestionType.MCQ, Grade = 10, ExamId = exam.Id, Order = 1, Choices = new HashSet<Choice> { new Choice { Text = "C#", IsCorrectAnswer = true, Order = 1 }, new Choice { Text = "Java", IsCorrectAnswer = false, Order = 2 }, new Choice { Text = "Python", IsCorrectAnswer = false, Order = 3 }, new Choice { Text = "PHP", IsCorrectAnswer = false, Order = 4 } } },
                new Question { Text = "Which HTTP method creates new data?", Type = QuestionType.MCQ, Grade = 10, ExamId = exam.Id, Order = 2, Choices = new HashSet<Choice> { new Choice { Text = "POST", IsCorrectAnswer = true, Order = 1 }, new Choice { Text = "GET", IsCorrectAnswer = false, Order = 2 }, new Choice { Text = "PUT", IsCorrectAnswer = false, Order = 3 }, new Choice { Text = "DELETE", IsCorrectAnswer = false, Order = 4 } } },
                new Question { Text = "Which database is used in this project?", Type = QuestionType.MCQ, Grade = 10, ExamId = exam.Id, Order = 3, Choices = new HashSet<Choice> { new Choice { Text = "SQL Server", IsCorrectAnswer = true, Order = 1 }, new Choice { Text = "MySQL", IsCorrectAnswer = false, Order = 2 }, new Choice { Text = "PostgreSQL", IsCorrectAnswer = false, Order = 3 }, new Choice { Text = "MongoDB", IsCorrectAnswer = false, Order = 4 } } },
                new Question { Text = "What status code means success?", Type = QuestionType.MCQ, Grade = 10, ExamId = exam.Id, Order = 4, Choices = new HashSet<Choice> { new Choice { Text = "200", IsCorrectAnswer = true, Order = 1 }, new Choice { Text = "404", IsCorrectAnswer = false, Order = 2 }, new Choice { Text = "500", IsCorrectAnswer = false, Order = 3 }, new Choice { Text = "401", IsCorrectAnswer = false, Order = 4 } } },
                new Question { Text = "Which ORM is used in ASP.NET Core?", Type = QuestionType.MCQ, Grade = 10, ExamId = exam.Id, Order = 5, Choices = new HashSet<Choice> { new Choice { Text = "Entity Framework Core", IsCorrectAnswer = true, Order = 1 }, new Choice { Text = "Dapper", IsCorrectAnswer = false, Order = 2 }, new Choice { Text = "NHibernate", IsCorrectAnswer = false, Order = 3 }, new Choice { Text = "ADO.NET", IsCorrectAnswer = false, Order = 4 } } }
            };

            await context.Questions.AddRangeAsync(questions);
            await context.SaveChangesAsync();
        }

        private static async Task SeedScoringTestExamAsync(AppDbContext context)
        {
            if (await context.Exams.AnyAsync(e => e.Name == "Scoring Test Exam"))
                return;

            var course = await context.Courses.FirstOrDefaultAsync(c => c.Code == "CS201");
            var instructor = await context.Instructors.FirstOrDefaultAsync();

            if (course == null || instructor == null)
                return;

            var exam = new Exam.Domain.Entities.Exam
            {
                Name = "Scoring Test Exam",
                Description = "A special exam to test score calculation. 3 questions, 10 pts each.",
                StartDate = DateTime.UtcNow.AddMinutes(-5),
                DueDate = DateTime.UtcNow.AddDays(365),
                TotalQuestions = 3,
                TotalGrade = 30,
                Type = ExamType.Final,
                CourseID = course.Id,
                InstructorID = instructor.Id,
                IsPublished = true,
                PassingScore = 15,
                Settings = new ExamSettings
                {
                    DurationMinutes = 60,
                    ShuffleQuestions = false,
                    ShuffleChoices = false,
                    ShowResultAfterSubmit = true,
                    AllowReview = true,
                    MaxAttempts = 1
                }
            };
            await context.Exams.AddAsync(exam);
            await context.SaveChangesAsync();

            var questions = new List<Question>
            {
                new Question
                {
                    Text = "Score Test Q1: Correct is Choice A",
                    Type = QuestionType.MCQ,
                    Grade = 10,
                    ExamId = exam.Id,
                    Order = 1,
                    Choices = new HashSet<Choice>
                    {
                        new Choice { Text = "Choice A (Correct)", IsCorrectAnswer = true, Order = 1 },
                        new Choice { Text = "Choice B (Wrong)", IsCorrectAnswer = false, Order = 2 }
                    }
                },
                new Question
                {
                    Text = "Score Test Q2: Correct is Choice B",
                    Type = QuestionType.MCQ,
                    Grade = 10,
                    ExamId = exam.Id,
                    Order = 2,
                    Choices = new HashSet<Choice>
                    {
                        new Choice { Text = "Choice A (Wrong)", IsCorrectAnswer = false, Order = 1 },
                        new Choice { Text = "Choice B (Correct)", IsCorrectAnswer = true, Order = 2 }
                    }
                },
                new Question
                {
                    Text = "Score Test Q3: Correct is Choice A",
                    Type = QuestionType.MCQ,
                    Grade = 10,
                    ExamId = exam.Id,
                    Order = 3,
                    Choices = new HashSet<Choice>
                    {
                        new Choice { Text = "Choice A (Correct)", IsCorrectAnswer = true, Order = 1 },
                        new Choice { Text = "Choice B (Wrong)", IsCorrectAnswer = false, Order = 2 }
                    }
                }
            };

            await context.Questions.AddRangeAsync(questions);
            await context.SaveChangesAsync();

            // Auto-enroll the postman student if exists
            var student = await context.Students.FirstOrDefaultAsync(s => s.Email == "postman@example.com");
            if (student != null)
            {
                if (!await context.ExamStudents.AnyAsync(es => es.ExamId == exam.Id && es.StudentId == student.Id))
                {
                    context.ExamStudents.Add(new ExamStudent
                    {
                        ExamId = exam.Id,
                        StudentId = student.Id,
                        Status = ExamStatus.NotStarted,
                        StartDate = DateTime.UtcNow
                    });
                    await context.SaveChangesAsync();
                }
            }
        }

        private static async Task SeedComprehensiveExamAsync(AppDbContext context)
        {
            if (await context.Exams.AnyAsync(e => e.Name == "Comprehensive Score Test V2"))
                return;

            var course = await context.Courses.FirstOrDefaultAsync(c => c.Code == "CS201");
            var instructor = await context.Instructors.FirstOrDefaultAsync();

            if (course == null || instructor == null) return;

            var exam = new Exam.Domain.Entities.Exam
            {
                Name = "Comprehensive Score Test V2",
                Description = "A full 10-question exam to test the final scoring logic.",
                StartDate = DateTime.UtcNow.AddMinutes(-10),
                DueDate = DateTime.UtcNow.AddDays(365),
                TotalQuestions = 10,
                TotalGrade = 100,
                Type = ExamType.Final,
                CourseID = course.Id,
                InstructorID = instructor.Id,
                IsPublished = true,
                PassingScore = 50,
                Settings = new ExamSettings
                {
                    DurationMinutes = 120,
                    MaxAttempts = 10,
                    ShowResultAfterSubmit = true
                }
            };
            await context.Exams.AddAsync(exam);
            await context.SaveChangesAsync();

            var questions = new List<Question>();
            for (int i = 1; i <= 10; i++)
            {
                // Alternate correct answers between A (1), B (2), C (3), D (4)
                int correctOrder = (i % 4) + 1;

                var q = new Question
                {
                    Text = $"Comprehensive Question #{i}: What is the result of {i} + {i}?",
                    Type = QuestionType.MCQ,
                    Grade = 10,
                    ExamId = exam.Id,
                    Order = i,
                    Choices = new HashSet<Choice>
                    {
                        new Choice { Text = $"Result {i*2} (Option A)", IsCorrectAnswer = (correctOrder == 1), Order = 1 },
                        new Choice { Text = $"Result {i*2 + 1} (Option B)", IsCorrectAnswer = (correctOrder == 2), Order = 2 },
                        new Choice { Text = $"Result {i*2 - 1} (Option C)", IsCorrectAnswer = (correctOrder == 3), Order = 3 },
                        new Choice { Text = $"None of the above (Option D)", IsCorrectAnswer = (correctOrder == 4), Order = 4 }
                    }
                };
                questions.Add(q);
            }

            await context.Questions.AddRangeAsync(questions);
            await context.SaveChangesAsync();

            var student = await context.Students.FirstOrDefaultAsync(s => s.Email == "postman@example.com");
            if (student != null)
            {
                context.ExamStudents.Add(new ExamStudent
                {
                    ExamId = exam.Id,
                    StudentId = student.Id,
                    Status = ExamStatus.NotStarted,
                    StartDate = DateTime.UtcNow
                });
                await context.SaveChangesAsync();
            }
        }
    }
}




