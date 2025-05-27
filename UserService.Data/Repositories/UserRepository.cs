using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

//using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using UserService.Data.Entities;
using UserService.Data.Repositories.Interfaces;

namespace UserService.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString;

        public UserRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            //_connectionString = configuration["DefaultConnection"];  // directly access the key vault secret

        }

        public async Task<IEnumerable<UserEntity>> GetAllUsersAsync()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                return await connection.QueryAsync<UserEntity>("sp_GetAllUsers", commandType: CommandType.StoredProcedure);
            }
        }

        public async Task<UserEntity> GetUserByIdAsync(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var parameters = new DynamicParameters();
                parameters.Add("@Id", id);
                return await connection.QueryFirstOrDefaultAsync<UserEntity>("sp_GetUserById", parameters, commandType: CommandType.StoredProcedure);
            }
        }

        public async Task<UserEntity> GetUserByUsernameAsync(string username)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var parameters = new DynamicParameters();
                parameters.Add("@Username", username);
                return await connection.QueryFirstOrDefaultAsync<UserEntity>("sp_GetUserByUsername", parameters, commandType: CommandType.StoredProcedure);
            }
        }

        public async Task<UserEntity> GetUserByEmailAsync(string email)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var parameters = new DynamicParameters();
                parameters.Add("@Email", email);
                return await connection.QueryFirstOrDefaultAsync<UserEntity>("sp_GetUserByEmail", parameters, commandType: CommandType.StoredProcedure);
            }
        }

    

        public async Task<UserEntity> CreateUserAsync(UserEntity user)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var parameters = new DynamicParameters();
                parameters.Add("@Username", user.Username);
                parameters.Add("@PasswordHash", user.PasswordHash);
                parameters.Add("@Email", user.Email);
                parameters.Add("@FitnessGoal", user.FitnessGoal);
                parameters.Add("@Role", user.Role);

                return await connection.QueryFirstOrDefaultAsync<UserEntity>("sp_CreateUser", parameters, commandType: CommandType.StoredProcedure);
            }
        }

        public async Task<UserEntity> UpdateUserAsync(int id, string email, string passwordHash, string fitnessGoal, string role)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var parameters = new DynamicParameters();
                parameters.Add("@Id", id);
                parameters.Add("@Email", email);
                parameters.Add("@PasswordHash", passwordHash);
                parameters.Add("@FitnessGoal", fitnessGoal);
                parameters.Add("@Role", role);

                return await connection.QueryFirstOrDefaultAsync<UserEntity>("sp_UpdateUser", parameters, commandType: CommandType.StoredProcedure);
            }
        }


        public async Task<bool> SoftDeleteUserAsync(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var parameters = new DynamicParameters();
                parameters.Add("@Id", id);

                // Use a stored procedure that sets IsDeleted flag instead of removing the record
                var rowsAffected = await connection.ExecuteScalarAsync<int>("sp_DeleteUser", parameters, commandType: CommandType.StoredProcedure);
                return rowsAffected > 0;
            }
        }

      


        // Method to get all soft-deleted users
        public async Task<IEnumerable<UserEntity>> GetSoftDeletedUsersAsync()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Query to get only soft-deleted users
                var query = "SELECT * FROM Users WHERE IsDeleted = 1";
                return await connection.QueryAsync<UserEntity>(query);
            }
        }

        // Method to restore a soft-deleted user
        public async Task<bool> RestoreUserAsync(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var parameters = new DynamicParameters();
                parameters.Add("@Id", id);

                // Execute stored procedure for restoring users
                 var rowsAffected = await connection.QuerySingleAsync<int>(
                "sp_RestoreUser", parameters, commandType: CommandType.StoredProcedure);


                return rowsAffected > 0;
            }
        }
        public async Task<bool> UsernameExistsAsync(string username)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var parameters = new DynamicParameters();
                parameters.Add("@Username", username);

                var exists = await connection.ExecuteScalarAsync<int>("sp_UsernameExists", parameters, commandType: CommandType.StoredProcedure);
                return exists == 1;
            }
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var parameters = new DynamicParameters();
                parameters.Add("@Email", email);

                var exists = await connection.ExecuteScalarAsync<int>("sp_EmailExists", parameters, commandType: CommandType.StoredProcedure);
                return exists == 1;
            }
        }

        // Authenticate user by username and password hash
        public async Task<UserEntity> AuthenticateUser(string username, string passwordHash)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = "EXEC sp_AuthenticateUser @Username, @PasswordHash";
                return await connection.QueryFirstOrDefaultAsync<UserEntity>(
                    query,
                    new { Username = username, PasswordHash = passwordHash }
                );
            }
        }

    }
}
