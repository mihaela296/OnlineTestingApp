using System;

namespace OnlineTestingApp.Models
{
    public class Result
    {
        public int ResultId { get; set; }
        public int AttemptId { get; set; }
        public decimal Score { get; set; }
        public string? Feedback { get; set; }
        public int? ReviewedByUserId { get; set; }
        public DateTime? ReviewedAt { get; set; }

        // Навигационные свойства
        public TestAttempt? Attempt { get; set; }
        public User? ReviewedByUser { get; set; }
    }
}