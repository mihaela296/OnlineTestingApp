using System.Collections.Generic;
using static System.Net.Mime.MediaTypeNames;

namespace OnlineTestingApp.Models
{
    public class Subject
    {
        public int SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public string? Description { get; set; }

        // Навигационные свойства
        public ICollection<Category> Categories { get; set; } = new List<Category>();
        public ICollection<Test> Tests { get; set; } = new List<Test>();
    }
}