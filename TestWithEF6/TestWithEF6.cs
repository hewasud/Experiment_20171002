using System.Linq;
using TestWithEF6.Models;
using QueryableExtensions;

namespace TestWithEF6
{
    class TestWithEF6
    {
        public static void Main()
        {
            using (var context = new TestDatabaseEntities())
            {
                // Left outer join Student to Enrollments
                System.Console.WriteLine("Using GroupJoin And SelectMany:");
                var stdEnrolments = context.Students
                    .GroupJoin(context.Enrollments, s => s.StudentID, e => e.StudentID, (s, e) => new { s, e })
                    .SelectMany(s2 => s2.e.DefaultIfEmpty(), (s2, e) => new { s2.s, e })
                    .Select(s => new { s.s, s.e })
                    .ToList();

                foreach(var r in stdEnrolments)
                {
                    System.Console.WriteLine($"StudentId: {r.s.StudentID}, CourseId: {((r.e != null) ? r.e.CourseID.ToString() : "none")}");
                }

                System.Console.WriteLine("\n\n\nUsing LeftJoin Extension:");
                var stdEnrolments2 = context.Students
                    .LeftJoin(context.Enrollments, s => s.StudentID, e => e.StudentID, (s, e) => new { s, e })
                    .Select(s2 => new { s2.s, s2.e })
                    .ToList();

                foreach(var r in stdEnrolments2)
                {
                    System.Console.WriteLine($"StudentId: {r.s.StudentID}, CourseId: {((r.e != null) ? r.e.CourseID.ToString() : "none")}");
                }
            }
            System.Console.ReadLine();
        }
    }
}
