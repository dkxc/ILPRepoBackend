using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Command.SignInDto
{
    public class InitiatePasswordSetupDto
    {
        public string Email { get; set; } = string.Empty;
    }
}
