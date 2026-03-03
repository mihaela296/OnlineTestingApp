using System;

namespace OnlineTestingApp.Models
{
    public class UserGroup
    {
        public int UserGroupId { get; set; }
        public int UserId { get; set; }
        public int GroupId { get; set; }
        public DateTime JoinedAt { get; set; }

        // Навигационные свойства
        public User? User { get; set; }
        public Group? Group { get; set; }
    }
}