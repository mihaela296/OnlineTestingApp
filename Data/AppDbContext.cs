using Microsoft.EntityFrameworkCore;
using OnlineTestingApp.Models;

namespace OnlineTestingApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // DbSet для каждой таблицы (модели)
        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Profile> Profiles { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<UserGroup> UserGroups { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Test> Tests { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<TestAttempt> TestAttempts { get; set; }
        public DbSet<UserAnswer> UserAnswers { get; set; }
        public DbSet<Result> Results { get; set; }
        public DbSet<ProctoringLog> ProctoringLogs { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<UserDevice> Devices { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ============= НАСТРОЙКА ТАБЛИЦ И СВЯЗЕЙ =============
modelBuilder.Entity<Role>().HasKey(r => r.RoleId);
    modelBuilder.Entity<User>().HasKey(u => u.UserId);
    modelBuilder.Entity<Profile>().HasKey(p => p.ProfileId);
    modelBuilder.Entity<Group>().HasKey(g => g.GroupId);
    modelBuilder.Entity<UserGroup>().HasKey(ug => new { ug.UserId, ug.GroupId });
    modelBuilder.Entity<Subject>().HasKey(s => s.SubjectId);
    modelBuilder.Entity<Category>().HasKey(c => c.CategoryId);
    modelBuilder.Entity<Test>().HasKey(t => t.TestId);
    modelBuilder.Entity<Question>().HasKey(q => q.QuestionId);
    modelBuilder.Entity<Answer>().HasKey(a => a.AnswerId);
    modelBuilder.Entity<TestAttempt>().HasKey(ta => ta.AttemptId);
    modelBuilder.Entity<UserAnswer>().HasKey(ua => ua.UserAnswerId);
    modelBuilder.Entity<Result>().HasKey(r => r.ResultId);
    modelBuilder.Entity<ProctoringLog>().HasKey(pl => pl.LogId);  // ЭТО РЕШАЕТ ПРОБЛЕМУ!
    modelBuilder.Entity<Log>().HasKey(l => l.LogId);
    modelBuilder.Entity<Conversation>().HasKey(c => c.ConversationId);
    modelBuilder.Entity<Message>().HasKey(m => m.MessageId);
    modelBuilder.Entity<Notification>().HasKey(n => n.NotificationId);
    modelBuilder.Entity<UserDevice>().HasKey(ud => ud.DeviceId);
            // Roles
            modelBuilder.Entity<Role>()
                .HasIndex(r => r.RoleName)
                .IsUnique();

            // Users - индексы
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.RoleId);

            // Profiles - один-к-одному с User
            modelBuilder.Entity<Profile>()
                .HasOne(p => p.User)
                .WithOne(u => u.Profile)
                .HasForeignKey<Profile>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Groups
            modelBuilder.Entity<Group>()
                .Property(g => g.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            // UserGroups - составной ключ и индексы
            modelBuilder.Entity<UserGroup>()
                .HasKey(ug => new { ug.UserId, ug.GroupId });

            modelBuilder.Entity<UserGroup>()
                .HasOne(ug => ug.User)
                .WithMany(u => u.UserGroups)
                .HasForeignKey(ug => ug.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserGroup>()
                .HasOne(ug => ug.Group)
                .WithMany(g => g.UserGroups)
                .HasForeignKey(ug => ug.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            // Subjects и Categories
            modelBuilder.Entity<Category>()
                .HasOne(c => c.Subject)
                .WithMany(s => s.Categories)
                .HasForeignKey(c => c.SubjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // Tests - индексы и связи
            modelBuilder.Entity<Test>()
                .HasIndex(t => t.CreatedByUserId);

            modelBuilder.Entity<Test>()
                .HasIndex(t => t.CategoryId);

            modelBuilder.Entity<Test>()
                .HasOne(t => t.CreatedByUser)
                .WithMany(u => u.CreatedTests)
                .HasForeignKey(t => t.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict); // Не удалять тесты при удалении пользователя

            modelBuilder.Entity<Test>()
                .HasOne(t => t.Category)
                .WithMany(c => c.Tests)
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Test>()
                .HasOne(t => t.Subject)
                .WithMany(s => s.Tests)
                .HasForeignKey(t => t.SubjectId)
                .OnDelete(DeleteBehavior.SetNull);

            // Questions - индексы и связи
            modelBuilder.Entity<Question>()
                .HasIndex(q => q.TestId);

            modelBuilder.Entity<Question>()
                .HasOne(q => q.Test)
                .WithMany(t => t.Questions)
                .HasForeignKey(q => q.TestId)
                .OnDelete(DeleteBehavior.Cascade); // При удалении теста удаляются и вопросы

            // Answers - индексы и связи
            modelBuilder.Entity<Answer>()
                .HasIndex(a => a.QuestionId);

            modelBuilder.Entity<Answer>()
                .HasOne(a => a.Question)
                .WithMany(q => q.Answers)
                .HasForeignKey(a => a.QuestionId)
                .OnDelete(DeleteBehavior.Cascade); // При удалении вопроса удаляются и ответы

            // TestAttempts - индексы и связи
            modelBuilder.Entity<TestAttempt>()
                .HasIndex(ta => new { ta.UserId, ta.TestId });

            modelBuilder.Entity<TestAttempt>()
                .HasIndex(ta => ta.IsCompleted);

            modelBuilder.Entity<TestAttempt>()
                .HasOne(ta => ta.User)
                .WithMany(u => u.TestAttempts)
                .HasForeignKey(ta => ta.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TestAttempt>()
                .HasOne(ta => ta.Test)
                .WithMany(t => t.TestAttempts)
                .HasForeignKey(ta => ta.TestId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TestAttempt>()
                .HasOne(ta => ta.UserDevice)
                .WithMany(d => d.TestAttempts)
                .HasForeignKey(ta => ta.DeviceId)
                .OnDelete(DeleteBehavior.SetNull);

            // UserAnswers - индексы и связи
            modelBuilder.Entity<UserAnswer>()
                .HasIndex(ua => ua.AttemptId);

            modelBuilder.Entity<UserAnswer>()
                .HasOne(ua => ua.Attempt)
                .WithMany(ta => ta.UserAnswers)
                .HasForeignKey(ua => ua.AttemptId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserAnswer>()
                .HasOne(ua => ua.Question)
                .WithMany(q => q.UserAnswers)
                .HasForeignKey(ua => ua.QuestionId)
                .OnDelete(DeleteBehavior.Restrict); // Не удалять ответы при удалении вопроса

            modelBuilder.Entity<UserAnswer>()
                .HasOne(ua => ua.SelectedAnswer)
                .WithMany(a => a.UserAnswers)
                .HasForeignKey(ua => ua.SelectedAnswerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Results - один-к-одному с TestAttempt
            modelBuilder.Entity<Result>()
                .HasIndex(r => r.AttemptId)
                .IsUnique();

            modelBuilder.Entity<Result>()
                .HasOne(r => r.Attempt)
                .WithOne(ta => ta.Result)
                .HasForeignKey<Result>(r => r.AttemptId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Result>()
                .HasOne(r => r.ReviewedByUser)
                .WithMany(u => u.ReviewedResults)
                .HasForeignKey(r => r.ReviewedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // ProctoringLogs - ОЧЕНЬ ВАЖНО для диплома!
            modelBuilder.Entity<ProctoringLog>()
                .HasIndex(pl => pl.AttemptId);

            modelBuilder.Entity<ProctoringLog>()
                .HasIndex(pl => pl.EventTime);

            modelBuilder.Entity<ProctoringLog>()
                .HasOne(pl => pl.Attempt)
                .WithMany(ta => ta.ProctoringLogs)
                .HasForeignKey(pl => pl.AttemptId)
                .OnDelete(DeleteBehavior.Cascade);

            // Logs (общие)
            modelBuilder.Entity<Log>()
                .HasIndex(l => l.UserId);

            modelBuilder.Entity<Log>()
                .HasIndex(l => l.Timestamp);

            modelBuilder.Entity<Log>()
                .HasOne(l => l.User)
                .WithMany(u => u.Logs)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Conversations (чаты)
            modelBuilder.Entity<Conversation>()
                .HasIndex(c => new { c.StudentUserId, c.TeacherUserId })
                .IsUnique();

            // Проверка, что Student и Teacher - разные пользователи
            modelBuilder.Entity<Conversation>()
                .ToTable(t => t.HasCheckConstraint("CK_Conversation_DifferentUsers", "StudentUserId != TeacherUserId"));

            modelBuilder.Entity<Conversation>()
                .HasOne(c => c.StudentUser)
                .WithMany(u => u.ConversationsAsStudent)
                .HasForeignKey(c => c.StudentUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Conversation>()
                .HasOne(c => c.TeacherUser)
                .WithMany(u => u.ConversationsAsTeacher)
                .HasForeignKey(c => c.TeacherUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Messages
            modelBuilder.Entity<Message>()
                .HasIndex(m => m.ConversationId);

            modelBuilder.Entity<Message>()
                .HasIndex(m => m.SentAt);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Conversation)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.SenderUser)
                .WithMany(u => u.SentMessages)
                .HasForeignKey(m => m.SenderUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Notifications
            modelBuilder.Entity<Notification>()
                .HasIndex(n => new { n.UserId, n.IsRead });

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Devices
            modelBuilder.Entity<UserDevice>()
                .HasIndex(d => d.UserId);

            modelBuilder.Entity<UserDevice>()
                .HasOne(d => d.User)
                .WithMany(u => u.Devices)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ============= НАСТРОЙКА ТИПОВ ДАННЫХ =============

            // Для SQL Server, чтобы правильно обрабатывать decimal
            modelBuilder.Entity<TestAttempt>()
                .Property(ta => ta.Score)
                .HasPrecision(5, 2);

            modelBuilder.Entity<Result>()
                .Property(r => r.Score)
                .HasPrecision(5, 2);

            // Для JSON полей (можно хранить как строки)
            modelBuilder.Entity<ProctoringLog>()
                .Property(pl => pl.Details)
                .HasColumnType("nvarchar(max)");

            modelBuilder.Entity<Notification>()
                .Property(n => n.Data)
                .HasColumnType("nvarchar(max)");

            modelBuilder.Entity<Log>()
                .Property(l => l.Details)
                .HasColumnType("nvarchar(max)");

            base.OnModelCreating(modelBuilder);
        }
    }
}