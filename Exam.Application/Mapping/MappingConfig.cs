using AutoMapper;
using Exam.Application.Dto.Identity;
using Exam.Application.Dto.Choice;
using ExamDTOs = Exam.Application.Dto.Exam;
using Exam.Application.Dto.Question;
using Exam.Domain.Entities.Identity;
using Exam.Domain.Entities;
using Exam.Application.Dto.Department;
using Exam.Application.Dto.Instructor;
using Exam.Application.Dto.Course;
using Exam.Application.Dto.Student;
using System.Linq;

namespace Exam.Application.Mapping
{
    public class MappingConfig : Profile
    {
        public MappingConfig()
        {
            CreateMap<CreateUser, AppUser>()
                .ForMember(dest => dest.UserName,
                           opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.FirstName,
                           opt => opt.MapFrom(src => src.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? ""))
                .ForMember(dest => dest.LastName,
                           opt => opt.MapFrom(src => string.Join(" ", src.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries).Skip(1))));

            // =======================
            // Department Mapping
            // =======================
            CreateMap<Department, DepartmentDTO>();
            CreateMap<DepartmentCreateDTO, Department>();

            // =======================
            // Instructor Mapping
            // =======================
            CreateMap<Instructor, InstructorReadDTO>();
            CreateMap<InstructorCreateDTO, Instructor>();
            CreateMap<InstructorUpdateDTO, Instructor>();

            // =======================
            // Course Mapping
            // =======================
            CreateMap<Course, CourseDTO>();
            CreateMap<CourseCreateDTO, Course>();

            // =======================
            // Student Mapping
            // =======================
            CreateMap<Student, StudentDTO>();
            CreateMap<StudentCreateDTO, Student>();
            CreateMap<StudentUpdateDTO, Student>();

            // =======================
            // Question Mapping
            // =======================
            CreateMap<QuestionCreateDTO, Question>()
                .ForMember(dest => dest.Text,
                           opt => opt.MapFrom(src => src.Text))

                // درجة السؤال
                .ForMember(dest => dest.Grade,
                           opt => opt.MapFrom(src => src.Grade))

                // نوع السؤال (MCQ - TrueFalse)
                .ForMember(dest => dest.Type,
                           opt => opt.MapFrom(src => src.Type))

                // Choices هتتضاف لاحقاً
                .ForMember(dest => dest.Choices, opt => opt.Ignore())
                .ForMember(dest => dest.ExamQuestions, opt => opt.Ignore());


            // =======================
            // Choice Mapping
            // =======================
            CreateMap<ChoiceCreateDTO, Choice>()
                // نص الاختيار
                .ForMember(dest => dest.Text,
                           opt => opt.MapFrom(src => src.Text))

                // هل هو الإجابة الصحيحة
                .ForMember(dest => dest.IsCorrectAnswer,
                           opt => opt.MapFrom(src => src.IsCorrect))

                // السؤال بيتحدد في السيرفس مش من الـ DTO
                .ForMember(dest => dest.QuestionId, opt => opt.Ignore())
                .ForMember(dest => dest.Question, opt => opt.Ignore());


            // =======================
            // Exam Mapping
            // =======================
            CreateMap<ExamDTOs.ExamCreateDTO, global::Exam.Domain.Entities.Exam>()
                // اسم الامتحان
                .ForMember(dest => dest.Name,
                           opt => opt.MapFrom(src => src.Name))

                // الوصف
                .ForMember(dest => dest.Description,
                           opt => opt.MapFrom(src => src.Description))

                // وقت البداية
                .ForMember(dest => dest.StartDate,
                           opt => opt.MapFrom(src => src.StartDate))

                // وقت النهاية
                .ForMember(dest => dest.DueDate,
                           opt => opt.MapFrom(src => src.DueDate))

                // الإعدادات
                .ForMember(dest => dest.Settings,
                           opt => opt.MapFrom(src => src.Settings))

                // علاقات تتحدد في الـ Service
                .ForMember(dest => dest.CourseID, opt => opt.MapFrom(src => src.CourseId))
                .ForMember(dest => dest.InstructorID, opt => opt.MapFrom(src => src.InstructorId))
                .ForMember(dest => dest.ExamQuestions, opt => opt.Ignore())
                .ForMember(dest => dest.ExamStudents, opt => opt.Ignore());
        }
    }
}
