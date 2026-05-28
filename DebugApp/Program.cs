using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Exam.Infrastructure.Data;
using Exam.Domain.Entities;
using Exam.Domain.Enum;

namespace DebugApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var connString = ""Server=db49468.public.databaseasp.net;Database=db49468;User Id=db49468;Password=7f@HeR5?#tE9;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True;"";
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(connString)
                .Options;

            using (var context = new AppDbContext(options))
            {
                var submittedSessions = context.ExamStudents
                    .Include(es => es.ExamAnswers)
                        .ThenInclude(ea => ea.Choice)
                    .Include(es => es.ExamAnswers)
                        .ThenInclude(ea => ea.Question)
                    .Where(es => es.Status == ExamStatus.Submitted)
                    .ToList();
                
                Console.WriteLine(""Found "" + submittedSessions.Count + "" submitted sessions."");

                foreach (var session in submittedSessions)
                {
                    Console.WriteLine(""\n--- Session ID: "" + session.Id + "" ---"");
                    Console.WriteLine(""Stored Score: "" + session.Score);
                    
                    double manualScore = 0;
                    foreach(var answer in session.ExamAnswers)
                    {
                        if(answer.Choice != null)
                        {
                            bool isCorrect = answer.Choice.IsCorrectAnswer;
                            double grade = answer.Question != null ? answer.Question.Grade : 0;
                            
                            Console.WriteLine(""Q "" + answer.QuestionId + "": Choice "" + answer.ChoiceId + "" (IsCorrect: "" + isCorrect + "", Grade: "" + grade + "")"");
                            
                            if (isCorrect)
                            {
                                manualScore += grade;
                            }
                        }
                        else
                        {
                            Console.WriteLine(""Q "" + answer.QuestionId + "": Choice "" + answer.ChoiceId + "" is NULL in DB!"");
                        }
                    }
                    Console.WriteLine(""Calculated Score: "" + manualScore);
                }
            }
        }
    }
}
