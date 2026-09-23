using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QueueManagement.API.Data;
using QueueManagement.Application.DTOs.Auth;
using QueueManagement.Domain.Entities;
using QueueManagement.Services.service;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly JwtService _jwtService;
    private readonly IPasswordHasher<User> _passwordHasher;

    public AuthController(
        AppDbContext context,
        JwtService jwtService,
        IPasswordHasher<User> passwordHasher)
    {
        _context = context;
        _jwtService = jwtService;
        _passwordHasher = passwordHasher;
    }

    [HttpPost("login")]
    public IActionResult Login(LoginRequest request)
    {
        var user = _context.Users
            .FirstOrDefault(x => x.MobileNo == request.MobileNumber);

        if (user == null)
        {
            return Unauthorized("Invalid email or password");
        }

        var result = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            request.Password
        );

        if (result == PasswordVerificationResult.Failed)
        {
            return Unauthorized("Invalid email or password");
        }

        var token = _jwtService.GenerateToken(user);

        return Ok(new LoginResponse
        {
            Token = token,
            Name = user.Name,
            Role = user.Role
        });
    }
}