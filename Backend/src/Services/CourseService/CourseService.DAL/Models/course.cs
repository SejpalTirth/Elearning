using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseService.DAL.Models
{
    public class course
    {
        public int ID { get; set; }
        public string CourseName { get; set; } = null!;
        public string CourseDescription { get; set; } = null!;
    }
}
