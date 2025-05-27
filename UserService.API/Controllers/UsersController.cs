using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserService.Business.Dtos;
using UserService.Business.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;

namespace UserService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]  // Only Admin can see all users
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }



        [HttpGet("{id}")]
        [Authorize]  // Any authenticated user
        public async Task<IActionResult> GetUserById(int id)
        {
            // Check if user is requesting their own information or is an admin
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var isAdmin = User.IsInRole("Admin");

            if (id != currentUserId && !isAdmin)
                return Forbid();

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound();
            return Ok(user);
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] CreateUserDto createUserDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);


            var (success, message, user) = await _userService.CreateUserAsync(createUserDto);
            if (!success)
                return BadRequest(message);

            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
        }

        // Add an endpoint for creating admin users (only accessible by admins)
        [HttpPost("create-admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateAdmin([FromBody] CreateUserDto createUserDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            createUserDto.Role = "Admin";
            var (success, message, user) = await _userService.CreateUserAsync(createUserDto);

            if (!success)
                return BadRequest(message);

            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
        }


        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (success, message, token) = await _userService.AuthenticateAsync(loginDto);
            if (!success)
                return Unauthorized(message);

            return Ok(new { token = token });
        }


        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto updateUserDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check if user is updating their own information or is an admin
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            if (id != currentUserId && !User.IsInRole("Admin"))
                return Forbid();

            var (success, message, user) = await _userService.UpdateUserAsync(id, updateUserDto);
            if (!success)
                return BadRequest(message);

            return Ok(user);
        }

       

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteUser(int id)
        {
            // Check if user is deleting their own account or is an admin
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            if (id != currentUserId && !User.IsInRole("Admin"))
                return Forbid();

            var (success, message) = await _userService.SoftDeleteUserAsync(id);
            if (!success)
                return BadRequest(message);

            return Ok(new { message = message });
        }


        // Endpoint to get all soft-deleted users (Admin only)
        [HttpGet("deleted")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetSoftDeletedUsers()
        {
            var users = await _userService.GetSoftDeletedUsersAsync();
            return Ok(users);
        }

        // Endpoint to restore a soft-deleted user (Admin only)
        [HttpPost("{id}/restore")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RestoreUser(int id)
        {
            var (success, message) = await _userService.RestoreUserAsync(id);
            if (!success)
                return BadRequest(message);

            return Ok(new { message = message });
        }

       


    }
}
