using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserService.Data.Entities
{
    public class UserActionMessage
    {
        public int UserId { get; set; }
        public string Action { get; set; } // "SoftDelete" or "Restore"
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

}
