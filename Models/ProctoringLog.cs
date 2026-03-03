using System;

namespace OnlineTestingApp.Models
{
    public class ProctoringLog
    {
        public int LogId { get; set; }
        public int AttemptId { get; set; }
        public string EventType { get; set; } = string.Empty; // 'AppSwitch', 'TabSwitch', 'SuspiciousPattern', 'FocusLost'
        public DateTime EventTime { get; set; }
        public string? Details { get; set; } // JSON с деталями
        public int SeverityLevel { get; set; } = 1; // 1-Info, 2-Warning, 3-Violation

        // Навигационное свойство
        public TestAttempt? Attempt { get; set; }
    }
}