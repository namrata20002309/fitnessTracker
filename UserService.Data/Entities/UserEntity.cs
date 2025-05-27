using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserService.Data.Entities
{
    public class UserEntity
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Email { get; set; }
        public string FitnessGoal { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Role { get; set; } = "User"; // Default role = User
        public bool IsDeleted { get; set; }



    }
}


