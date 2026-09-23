using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

    [AllowAnonymous]
    [HttpPost("signup")]
    public async Task<IActionResult> Signup(SignupRequest request)
    {
        if (await _context.Users.AnyAsync(x => x.MobileNo == request.MobileNumber))
        {
            return Conflict(new { message = "A user with this mobile number already exists." });
        }

        var user = new User
        {
            Name = request.Name.Trim(),
            MobileNo = request.MobileNumber.Trim(),
            Role = "User"
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return StatusCode(StatusCodes.Status201Created, new
        {
            message = "Signup successful. Please login to receive a token."
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("admin/signup")]
    public async Task<IActionResult> SignupAdmin(SignupRequest request)
    {
        if (await _context.Users.AnyAsync(x => x.MobileNo == request.MobileNumber))
        {
            return Conflict(new { message = "A user with this mobile number already exists." });
        }

        var admin = new User
        {
            Name = request.Name.Trim(),
            MobileNo = request.MobileNumber.Trim(),
            Role = "Admin"
        };
        admin.PasswordHash = _passwordHasher.HashPassword(admin, request.Password);

        _context.Users.Add(admin);
        await _context.SaveChangesAsync();

        return StatusCode(StatusCodes.Status201Created, new
        {
            message = "Admin signup successful."
        });
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await _context.Users
            .SingleOrDefaultAsync(x => x.MobileNo == request.MobileNumber.Trim());

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