using OnlineTestingApp.Views;
using OnlineTestingApp.Views.Auth;
using OnlineTestingApp.Views.Admin;


namespace OnlineTestingApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        
        // Регистрируем маршруты
        Routing.RegisterRoute("LoginPage", typeof(LoginPage));
        Routing.RegisterRoute("RegisterPage", typeof(RegisterPage));
        Routing.RegisterRoute("StudentDashboardPage", typeof(StudentDashboardPage));
        Routing.RegisterRoute("TeacherDashboardPage", typeof(TeacherDashboardPage));
        Routing.RegisterRoute("PendingGroupPage", typeof(PendingGroupPage));
        Routing.RegisterRoute("PendingApprovalPage", typeof(PendingApprovalPage));
        Routing.RegisterRoute("ForgotPasswordPage", typeof(ForgotPasswordPage));
        Routing.RegisterRoute("VerifyCodePage", typeof(VerifyCodePage));
        Routing.RegisterRoute("UserManagementPage", typeof(Views.Admin.UserManagementPage));
        Routing.RegisterRoute("PendingGroupPage", typeof(PendingGroupPage));
        Routing.RegisterRoute("PendingApprovalPage", typeof(PendingApprovalPage));
        Routing.RegisterRoute("ForgotPasswordPage", typeof(ForgotPasswordPage));
       
    }
}
