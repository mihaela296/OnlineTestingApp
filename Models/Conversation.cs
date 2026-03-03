using System;
using System.Collections.Generic;

namespace OnlineTestingApp.Models
{
    public class Conversation
    {
        public int ConversationId { get; set; }
        public int StudentUserId { get; set; }
        public int TeacherUserId { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public bool IsActive { get; set; } = true;

        // Навигационные свойства
        public User? StudentUser { get; set; }
        public User? TeacherUser { get; set; }
        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}