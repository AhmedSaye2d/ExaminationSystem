using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exam.Application.Dto.Choice
{
    public class ChoiceForStudentDTO
    {
        public int Id { get; set; }
        // معرف الاختيار

        public string Text { get; set; } = string.Empty;
        // نص الاختيار الظاهر للطالب
    }
}
