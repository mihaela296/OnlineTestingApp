namespace OnlineTestingApp.Models
{
    public class Profile
    {
        public int ProfileId { get; set; }
        public int UserId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }
        public string? PhoneNumber { get; set; }

        // Навигационное свойство
        public User? User { get; set; }
    }
}