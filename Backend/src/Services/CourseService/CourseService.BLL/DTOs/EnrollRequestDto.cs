using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseService.BLL.DTOs
{
    public class EnrollRequestDto
    {
        public int CourseId { get; set; }
        public string UserId { get; set; }
    }

}
