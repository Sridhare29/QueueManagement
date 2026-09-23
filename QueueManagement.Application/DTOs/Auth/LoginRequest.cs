using System.ComponentModel.DataAnnotations;

namespace QueueManagement.Application.DTOs.Auth
{
    public class LoginRequest
    {
        [Required]
        public string MobileNumber { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
