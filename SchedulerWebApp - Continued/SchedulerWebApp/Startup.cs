using Hangfire;
using Hangfire.Dashboard;
using Microsoft.Owin;
using Owin;
using SchedulerWebApp;

[assembly: OwinStartup(typeof(Startup))]

namespace SchedulerWebApp
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {

            var authorizationFilter = new BasicAuthAuthorizationFilter(
                                new BasicAuthAuthorizationFilterOptions
                                {
                                    // Require secure connection for dashboard
                                    RequireSsl = true,
                                    // Case sensitive login checking
                                    LoginCaseSensitive = true,
                                    // Users
                                    Users = new[]
                                    {
                                        new BasicAuthAuthorizationUser
                                        {
                                            Login = "admin@scheduler.com",
                                            PasswordClear = "passW0rd!"
                                        }
                                    }
                                });

            var options = new DashboardOptions
            {
                AuthorizationFilters = new IAuthorizationFilter[]
                {
                    authorizationFilter
                }
            };
            GlobalConfiguration.Configuration.UseSqlServerStorage("DefaultConnection");

            app.UseHangfireDashboard("/hangfire", options);
            app.UseHangfireServer();

            ConfigureAuth(app);
        }
    }
}