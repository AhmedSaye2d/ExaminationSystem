using Exam.Domain.Entities.Common;
using System.Collections.Generic;

namespace Exam.Domain.Entities
{
    public class Subject : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        
        // Relationships
        public ICollection<Exam> Exams { get; set; } = new List<Exam>();
    }
}
