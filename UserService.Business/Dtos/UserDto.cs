using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserService.Business.Dtos
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string FitnessGoal { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Role { get; set; }

    }

    public class CreateUserDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string? FitnessGoal { get; set; }
        public string Role { get; set; } = "User"; // Default role

    }

    public class LoginDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class UpdateUserDto
    {
        [MinLength(8)]
        public string Password { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        public string FitnessGoal { get; set; }

        public string Role { get; set; } // Add this

    }

    public class ForgotPasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }

    public class ResetPasswordDto
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string ResetToken { get; set; }

        [Required]
        [MinLength(8)]
        public string NewPassword { get; set; }
    }

    public class ChangePasswordDto
    {
        [Required]
        public string CurrentPassword { get; set; }

        [Required]
        [MinLength(8)]
        public string NewPassword { get; set; }
    }

}
