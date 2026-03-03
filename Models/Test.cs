using System;
using System.Collections.Generic;

namespace OnlineTestingApp.Models
{
    public class Test
    {
        public int TestId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DurationMinutes { get; set; }
        public int CreatedByUserId { get; set; }
        public int? CategoryId { get; set; }
        public int? SubjectId { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public int PassingScore { get; set; } = 60;
        public int MaxAttempts { get; set; } = 1;
        public bool IsRandomized { get; set; } = false;
        public bool ShowResultsImmediately { get; set; } = true;

        // Навигационные свойства
        public User? CreatedByUser { get; set; }
        public Category? Category { get; set; }
        public Subject? Subject { get; set; }
        public ICollection<Question> Questions { get; set; } = new List<Question>();
        public ICollection<TestAttempt> TestAttempts { get; set; } = new List<TestAttempt>();
    }
}