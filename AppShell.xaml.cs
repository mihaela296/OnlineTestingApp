using OnlineTestingApp.Views;
using OnlineTestingApp.Views.Auth;

namespace OnlineTestingApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        
        // Регистрируем ВСЕ маршруты для навигации
        Routing.RegisterRoute("RegisterPage", typeof(RegisterPage));
        Routing.RegisterRoute("StudentDashboardPage", typeof(StudentDashboardPage));
        Routing.RegisterRoute("TeacherDashboardPage", typeof(TeacherDashboardPage));
        Routing.RegisterRoute("AdminDashboardPage", typeof(AdminDashboardPage));
        Routing.RegisterRoute("PendingGroupPage", typeof(PendingGroupPage));
        Routing.RegisterRoute("PendingApprovalPage", typeof(PendingApprovalPage));
        Routing.RegisterRoute("TestDbPage", typeof(TestDbPage));
    }
}
