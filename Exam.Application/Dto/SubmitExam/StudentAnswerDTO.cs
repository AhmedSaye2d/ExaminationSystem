using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exam.Application.Dto.SubmitExam
{
    public class StudentAnswerDTO
    {
        public int QuestionId { get; set; }
        // معرف السؤال

        public int ChoiceId { get; set; }
        // الاختيار الذي حدده الطالب
    }
}
