using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskboardApi.Data;
using TaskboardApi.Helpers;
using TaskboardApi.Models;

namespace TaskboardApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public AuthController(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest("Username and password are required.");
        if (await _db.Users.AnyAsync(u => u.Username == req.Username))
            return Conflict("Username already exists.");

        var salt = AuthHelper.GenerateSalt();
        var user = new User
        {
            Username = req.Username.Trim(),
            Salt = salt,
            PasswordHash = AuthHelper.HashPassword(req.Password, salt),
            Role = AuthHelper.NormalizeRole(req.Role)
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return Ok(new AuthResponse(AuthHelper.GenerateJwt(user, _config), user.Username, user.Role));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest req)
    {
        var user = await _db.Users.SingleOrDefaultAsync(u => u.Username == req.Username);
        if (user is null || !AuthHelper.Verify(req.Password, user.Salt, user.PasswordHash))
            return Unauthorized("Invalid credentials.");
        return Ok(new AuthResponse(AuthHelper.GenerateJwt(user, _config), user.Username, user.Role));
    }
}
