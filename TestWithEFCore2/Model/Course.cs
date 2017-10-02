using System;
using System.Collections.Generic;

namespace TestWithEFCore2.Model
{
    public partial class Course
    {
        public Course()
        {
            Enrollment = new HashSet<Enrollment>();
        }

        public int CourseId { get; set; }
        public string Title { get; set; }
        public int? Credits { get; set; }

        public ICollection<Enrollment> Enrollment { get; set; }
    }
}
