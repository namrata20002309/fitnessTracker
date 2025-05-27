using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserService.Data.Entities;

namespace UserService.Business.Services.Interfaces
{
    public interface IUserActionQueueService
    {
        Task SendMessageAsync(UserActionMessage message);
        Task EnsureQueueExistsAsync();
    }
}
