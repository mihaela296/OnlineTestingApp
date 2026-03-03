using System;

namespace OnlineTestingApp.Models
{
    public class Log
    {
        public int LogId { get; set; }
        public int? UserId { get; set; }
        public string Action { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string? Details { get; set; }
        public string? IPAddress { get; set; }

        // Навигационное свойство
        public User? User { get; set; }
    }
}