using System;
using System.Collections.Generic;

namespace OnlineTestingApp.Models
{
    public class Role
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;

        // Навигационные свойства
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
