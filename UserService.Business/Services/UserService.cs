using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using UserService.Business.Dtos;
using UserService.Business.Services.Interfaces;
using UserService.Data.Entities;
using UserService.Data.Repositories;
using UserService.Data.Repositories.Interfaces;
using BC = BCrypt.Net.BCrypt;


namespace UserService.Business.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly IUserActionQueueService _queueService; // Change this line to use interface



        public UserService(IUserRepository userRepository, IConfiguration configuration, IUserActionQueueService queueService)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _queueService = queueService;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllUsersAsync();
            return users.Select(MapToDto);
        }

        public async Task<UserDto> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            return user != null ? MapToDto(user) : null;
        }

        public async Task<UserDto> GetUserByUsernameAsync(string username)
        {
            var user = await _userRepository.GetUserByUsernameAsync(username);
            return user != null ? MapToDto(user) : null;
        }

        public async Task<(bool Success, string Message, UserDto User)> CreateUserAsync(CreateUserDto createUserDto)
        {
            // Check if username exists
            if (await _userRepository.UsernameExistsAsync(createUserDto.Username))
                return (false, "Username already exists", null);

            // Check if email exists
            if (await _userRepository.EmailExistsAsync(createUserDto.Email))
                return (false, "Email already exists", null);

            // Create new user
            var user = new UserEntity
            {
                Username = createUserDto.Username,
                PasswordHash = BC.HashPassword(createUserDto.Password),
                Email = createUserDto.Email,
                FitnessGoal = createUserDto.FitnessGoal,
                Role = createUserDto.Role ?? "User", // Set role
                CreatedAt = DateTime.UtcNow
            };

            var createdUser = await _userRepository.CreateUserAsync(user);
            return createdUser != null
                ? (true, "User created successfully", MapToDto(createdUser))
                : (false, "Failed to create user", null);
        }


        public async Task<(bool Success, string Message, UserDto User)> UpdateUserAsync(int id, UpdateUserDto updateUserDto)
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null)
                return (false, "User not found", null);

            // Check if email exists if changing email
            if (!string.IsNullOrEmpty(updateUserDto.Email) && user.Email != updateUserDto.Email)
            {
                if (await _userRepository.EmailExistsAsync(updateUserDto.Email))
                    return (false, "Email already in use", null);
            }

            // Calculate what needs to be updated
            string passwordHash = !string.IsNullOrEmpty(updateUserDto.Password)
                ? BC.HashPassword(updateUserDto.Password)
                : null;

            // Update user
            var updatedUser = await _userRepository.UpdateUserAsync(
                id,
                updateUserDto.Email,
                passwordHash,
                updateUserDto.FitnessGoal,
                updateUserDto.Role
            );

            return updatedUser != null
                ? (true, "User updated successfully", MapToDto(updatedUser))
                : (false, "Failed to update user", null);
        }

        private string GenerateJwtToken(UserEntity user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role) // Add role claim
        }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private UserDto MapToDto(UserEntity user)
        {
            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FitnessGoal = user.FitnessGoal,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<(bool Success, string Message)> SoftDeleteUserAsync(int id)
        {
            var result = await _userRepository.SoftDeleteUserAsync(id);

            if (result)
            {
                // Send message to queue for WorkoutService to disable workouts
                var message = new UserActionMessage { UserId = id, Action = "SoftDelete" };
                await _queueService.SendMessageAsync(message);

                return (true, "User deactivated successfully");
            }
            return (false, "User not found or failed to deactivate");
        }

      
        public async Task<(bool Success, string Message)> RestoreUserAsync(int id)
        {
            var result = await _userRepository.RestoreUserAsync(id);

            if (result)
            {
                // Send message to queue for WorkoutService to restore workouts
                var message = new UserActionMessage { UserId = id, Action = "Restore" };
                await _queueService.SendMessageAsync(message);

                return (true, "User restored successfully");
            }
            return (false, "User not found or failed to restore");
        }

        // Method to get all soft-deleted users
        public async Task<IEnumerable<UserDto>> GetSoftDeletedUsersAsync()
        {
            var users = await _userRepository.GetSoftDeletedUsersAsync();
            return users.Select(MapToDto);
        }



        public async Task<(bool Success, string Message, string Token)> AuthenticateAsync(LoginDto loginDto)
        {
            var user = await _userRepository.GetUserByUsernameAsync(loginDto.Username);

            // Check if user exists and password is correct
            if (user == null || !BC.Verify(loginDto.Password, user.PasswordHash))
                return (false, "Invalid username or password", null);

            // Generate JWT token
            var token = GenerateJwtToken(user);
            return (true, "Authentication successful", token);
        }


    }
}
































    
