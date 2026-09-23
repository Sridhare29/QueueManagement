using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueManagement.Application.DTOs.Auth
{
    public class LoginRequest
    {
        public string MobileNumber { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }
}
