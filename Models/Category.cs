using System.Collections.Generic;
using static System.Net.Mime.MediaTypeNames;

namespace OnlineTestingApp.Models
{
    public class Category
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int SubjectId { get; set; }

        // Навигационные свойства
        public Subject? Subject { get; set; }
        public ICollection<Test> Tests { get; set; } = new List<Test>();
    }
}