using Exam.Application.Dto.Exam;
using FluentValidation;

namespace Exam.Application.Validation.Exam
{
    public class ExamCreateValidator : AbstractValidator<ExamCreateDTO>
    {
        public ExamCreateValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Exam name is required.")
                .MaximumLength(200);

            RuleFor(x => x.StartDate)
                .NotEmpty().WithMessage("Start date is required.");

            RuleFor(x => x.DueDate)
                .NotEmpty().WithMessage("Due date is required.")
                .GreaterThan(x => x.StartDate).WithMessage("Due date must be after start date.");

            RuleFor(x => x.TotalGrade)
                .GreaterThan(0).WithMessage("Total grade must be greater than 0.");

            RuleFor(x => x.PassingScore)
                .GreaterThanOrEqualTo(0).WithMessage("Passing score cannot be negative.")
                .LessThanOrEqualTo(x => x.TotalGrade).WithMessage("Passing score cannot exceed total grade.");

            RuleFor(x => x.CourseId)
                .GreaterThan(0).WithMessage("Valid Course ID is required.");

            RuleFor(x => x.InstructorId)
                .GreaterThan(0).WithMessage("Valid Instructor ID is required.");
        }
    }
}
