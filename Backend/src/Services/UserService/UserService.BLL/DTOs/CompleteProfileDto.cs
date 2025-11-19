using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserService.BLL.DTOs
{
    public class CompleteProfileDto
    {
        public Guid UserId { get; set; }
        public string Name { get; set; } = default!;
        public int RoleId { get; set; }
    }

}
