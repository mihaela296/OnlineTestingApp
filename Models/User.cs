using System;
using System.Collections.Generic;

namespace OnlineTestingApp.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public bool IsActive { get; set; } = true;

        // Навигационные свойства
        public Role? Role { get; set; }
        public Profile? Profile { get; set; }
        public ICollection<UserGroup> UserGroups { get; set; } = new List<UserGroup>();
        public ICollection<Test> CreatedTests { get; set; } = new List<Test>();
        public ICollection<TestAttempt> TestAttempts { get; set; } = new List<TestAttempt>();
        public ICollection<Result> ReviewedResults { get; set; } = new List<Result>();
        public ICollection<Log> Logs { get; set; } = new List<Log>();
        public ICollection<Conversation> ConversationsAsStudent { get; set; } = new List<Conversation>();
        public ICollection<Conversation> ConversationsAsTeacher { get; set; } = new List<Conversation>();
        public ICollection<Message> SentMessages { get; set; } = new List<Message>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<UserDevice> Devices { get; set; } = new List<UserDevice>();
    }
}
