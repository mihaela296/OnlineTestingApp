using System.Collections.Generic;

namespace OnlineTestingApp.Models
{
    public class Question
    {
        public int QuestionId { get; set; }
        public int TestId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string QuestionType { get; set; } = string.Empty; // 'MultipleChoice', 'TrueFalse', 'OpenAnswer'
        public int Points { get; set; } = 1;
        public int OrderIndex { get; set; } = 0;
        public string? ImageUrl { get; set; }

        // Навигационные свойства
        public Test? Test { get; set; }
        public ICollection<Answer> Answers { get; set; } = new List<Answer>();
        public ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
    }
}