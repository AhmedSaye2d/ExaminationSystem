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
using Exam.Application.Dto.SubmitExam;
using Exam.Domain.Enum;
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
                .ForMember(dest => dest.Email,
                           opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.UserType,
                           opt => opt.MapFrom(src => src.UserType))
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
            CreateMap<Student, StudentDTO>()
                .ForMember(dest => dest.EnrolledCourses, 
                           opt => opt.MapFrom(src => src.CourseStudents.Select(cs => cs.Course)));
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
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type));

            CreateMap<QuestionWithChoicesDTO, Question>()
                .ForMember(dest => dest.Choices, opt => opt.MapFrom(src => src.Choices))
                // Choices يتم تحويلها تلقائياً بالاعتماد على ماب ChoiceCreateDTO
                .ForMember(dest => dest.Exam, opt => opt.Ignore());

            CreateMap<Question, QuestionDTO>();

            CreateMap<Question, QuestionReadDTO>();

            CreateMap<Question, QuestionForExamDTO>();

            CreateMap<QuestionUpdateDTO, Question>()
                .ForMember(dest => dest.Choices, opt => opt.Ignore())
                .ForMember(dest => dest.Exam, opt => opt.Ignore());

            CreateMap<QuestionWithChoicesDTO, Question>()
                .ForMember(dest => dest.Choices, opt => opt.MapFrom(src => src.Choices))
                .ForMember(dest => dest.Exam, opt => opt.Ignore());

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

            CreateMap<Choice, ChoiceDTO>()
                .ForMember(dest => dest.IsCorrect, opt => opt.MapFrom(src => src.IsCorrectAnswer));

            CreateMap<Choice, ChoiceReadDTO>()
                .ForMember(dest => dest.IsCorrect, opt => opt.MapFrom(src => src.IsCorrectAnswer))
                .ForMember(dest => dest.QuestionId, opt => opt.MapFrom(src => src.QuestionId));

            CreateMap<Choice, Dto.Choice.ChoiceForStudentDTO>();

            CreateMap<ChoiceUpdateDTO, Choice>()
                .ForMember(dest => dest.IsCorrectAnswer, opt => opt.MapFrom(src => src.IsCorrect))
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
                .ForMember(dest => dest.Questions, opt => opt.Ignore())
                .ForMember(dest => dest.ExamStudents, opt => opt.Ignore());

            CreateMap<global::Exam.Domain.Entities.Exam, ExamDTOs.ExamDTO>()
                .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => src.CourseID))
                .ForMember(dest => dest.InstructorId, opt => opt.MapFrom(src => src.InstructorID));

            // =======================
            // Question & Choice (Read/Management) Mappings
            // =======================
            CreateMap<Choice, ChoiceDTO>()
                .ForMember(dest => dest.IsCorrect, opt => opt.MapFrom(src => src.IsCorrectAnswer));
            
            CreateMap<Choice, ChoiceReadDTO>()
                 .ForMember(dest => dest.IsCorrect, opt => opt.MapFrom(src => src.IsCorrectAnswer));

            CreateMap<Choice, ChoiceForStudentDTO>();

            CreateMap<ChoiceUpdateDTO, Choice>()
                .ForMember(dest => dest.IsCorrectAnswer, opt => opt.MapFrom(src => src.IsCorrect));

            CreateMap<Question, QuestionDTO>();
            CreateMap<Question, QuestionReadDTO>();
            CreateMap<Question, QuestionForStudentDTO>();
            CreateMap<QuestionUpdateDTO, Question>();

            // =======================
            // Exam Results Mappings
            // =======================
            CreateMap<ExamStudent, ExamResultDTO>()
                .ForMember(dest => dest.ExamName, opt => opt.MapFrom(src => src.Exam != null ? src.Exam.Name : ""))
                .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => src.Student != null ? (src.Student.FirstName + " " + src.Student.LastName) : ""))
                .ForMember(dest => dest.TotalGrade, opt => opt.MapFrom(src => src.Exam != null ? src.Exam.TotalGrade : 0));

        }
    }
}
