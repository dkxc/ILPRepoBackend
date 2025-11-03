using IlpRepoBackend.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Dto
{
    public class LoginResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public int UserId { get; set; }
        public UserRole RoleName { get; set; } 
    }
}
