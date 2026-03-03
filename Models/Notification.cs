using System;

namespace OnlineTestingApp.Models
{
    public class Notification
    {
        public int NotificationId { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string NotificationText { get; set; } = string.Empty;
        public string NotificationType { get; set; } = string.Empty; // 'NewTest', 'TestResult', 'NewMessage'
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }
        public string? Data { get; set; } // JSON с дополнительными данными

        // Навигационное свойство
        public User? User { get; set; }
    }
}