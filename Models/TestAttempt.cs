using System;
using System.Collections.Generic;

namespace OnlineTestingApp.Models
{
    public class TestAttempt
    {
        public int AttemptId { get; set; }
        public int UserId { get; set; }
        public int TestId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool IsCompleted { get; set; } = false;
        public decimal? Score { get; set; }
        public bool? IsPassed { get; set; }
        public int AttemptNumber { get; set; } = 1;
        public int? DeviceId { get; set; }

        // Навигационные свойства
        public User? User { get; set; }
        public Test? Test { get; set; }
        public UserDevice? UserDevice { get; set; }  // Связь с устройством
        public ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
        public Result? Result { get; set; }
        public ICollection<ProctoringLog> ProctoringLogs { get; set; } = new List<ProctoringLog>();
    }
}