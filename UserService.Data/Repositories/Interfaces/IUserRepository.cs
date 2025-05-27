using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserService.Data.Entities;

namespace UserService.Data.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<UserEntity> CreateUserAsync(UserEntity user);
        Task<UserEntity> GetUserByUsernameAsync(string username);
        Task<UserEntity> GetUserByIdAsync(int id);

        Task<UserEntity> AuthenticateUser(string username, string passwordHash);

        Task<IEnumerable<UserEntity>> GetAllUsersAsync();
        Task<UserEntity> GetUserByEmailAsync(string email);
        Task<UserEntity> UpdateUserAsync(int id, string email, string passwordHash, string fitnessGoal, string role);
     
        Task<bool> UsernameExistsAsync(string username);
        Task<bool> EmailExistsAsync(string email);

        Task<bool> SoftDeleteUserAsync(int id);

        Task<IEnumerable<UserEntity>> GetSoftDeletedUsersAsync();
        Task<bool> RestoreUserAsync(int id);

    }
}
