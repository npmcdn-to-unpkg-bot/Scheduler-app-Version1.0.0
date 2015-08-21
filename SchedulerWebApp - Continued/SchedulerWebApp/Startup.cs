using Hangfire;
using Microsoft.Owin;
using Owin;
using SchedulerWebApp;

[assembly: OwinStartup(typeof (Startup))]

namespace SchedulerWebApp
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            GlobalConfiguration.Configuration.UseSqlServerStorage("DefaultConnection");
            app.UseHangfireDashboard();

            ConfigureAuth(app);
        }
    }
}