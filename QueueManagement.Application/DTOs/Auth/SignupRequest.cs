using System.ComponentModel.DataAnnotations;

namespace QueueManagement.Application.DTOs.Auth;

public class SignupRequest
{
    [Required, StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(20, MinimumLength = 3)]
    public string MobileNumber { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string Password { get; set; } = string.Empty;
}