using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserService.Business.Dtos;

namespace UserService.Business.Services.Interfaces
{
    public interface IUserService
    {
        //Task<UserDto> RegisterUser(CreateUserDto userDto);
        //Task<UserDto> AuthenticateUser(string username, string password);


        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<UserDto> GetUserByIdAsync(int id);
        Task<UserDto> GetUserByUsernameAsync(string username);
        Task<(bool Success, string Message, UserDto User)> CreateUserAsync(CreateUserDto createUserDto);
        Task<(bool Success, string Message, UserDto User)> UpdateUserAsync(int id, UpdateUserDto updateUserDto);
        //Task<(bool Success, string Message)> DeleteUserAsync(int id);
        Task<(bool Success, string Message, string Token)> AuthenticateAsync(LoginDto loginDto);

        Task<(bool Success, string Message)> SoftDeleteUserAsync(int id);

        Task<IEnumerable<UserDto>> GetSoftDeletedUsersAsync();
        Task<(bool Success, string Message)> RestoreUserAsync(int id);
       
    }
}

        