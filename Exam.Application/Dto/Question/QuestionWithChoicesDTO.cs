using Exam.Application.Dto.Choice;
using Exam.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exam.Application.Dto.Question
{
    public class QuestionWithChoicesDTO
    {
        public int ExamId { get; set; }
        public string Text { get; set; } = string.Empty;

        public int Grade { get; set; }

        public QuestionType Type { get; set; }

        public List<ChoiceCreateDTO> Choices { get; set; } = new();
    }
}
