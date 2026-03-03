namespace OnlineTestingApp.Models
{
    public class UserAnswer
    {
        public int UserAnswerId { get; set; }
        public int AttemptId { get; set; }
        public int QuestionId { get; set; }
        public int? SelectedAnswerId { get; set; }
        public string? TextAnswer { get; set; }
        public bool? IsCorrect { get; set; }
        public int? TimeSpentSeconds { get; set; }

        // Навигационные свойства
        public TestAttempt? Attempt { get; set; }
        public Question? Question { get; set; }
        public Answer? SelectedAnswer { get; set; }
    }
}