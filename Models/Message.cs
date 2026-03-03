using System;

namespace OnlineTestingApp.Models
{
    public class Message
    {
        public int MessageId { get; set; }
        public int ConversationId { get; set; }
        public int SenderUserId { get; set; }
        public string MessageText { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime? ReadAt { get; set; }

        // Навигационные свойства
        public Conversation? Conversation { get; set; }
        public User? SenderUser { get; set; }
    }
}