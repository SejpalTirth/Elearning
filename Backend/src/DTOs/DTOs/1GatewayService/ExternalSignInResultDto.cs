using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs._1GatewayService
{
    public class ExternalSignInResultDto
    {
        public bool IsNewUser { get; set; }
        public Guid? UserId { get; set; }
        public bool IsAdmin { get; set; }
        public TokenResponseDto? Tokens { get; set; }
    }

}
