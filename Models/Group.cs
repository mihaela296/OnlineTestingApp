using System;
using System.Collections.Generic;

namespace OnlineTestingApp.Models
{
    public class Group
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }

        // Навигационные свойства
        public ICollection<UserGroup> UserGroups { get; set; } = new List<UserGroup>();
    }
}