using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QueueManagement.Application.DTOs;
using QueueManagement.Domain.Entities;
using QueueManagement.API.Data;

namespace QueueManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly AppDbContext _context;

    public UserController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _context.Users.ToListAsync();

        return Ok(users);
    }
}