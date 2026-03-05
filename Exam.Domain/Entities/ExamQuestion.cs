using Exam.Domain.Common;
using Exam.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exam.Domain.Entities
{
    public class ExamQuestion : BaseEntity
    {
        public int ExamId { get; set; }
        public Exam? Exam { get; set; }

        public int QuestionId { get; set; }
        public Question? Question { get; set; }

        public int Points { get; set; }   // درجة السؤال
        public int Order { get; set; }    // ترتيب العرض
    }

}
