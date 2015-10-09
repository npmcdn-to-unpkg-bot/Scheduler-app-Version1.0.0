using Hangfire;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
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
            app.UseHangfireServer();

            var provider = new CookieAuthenticationProvider {OnException = context => { }};

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                Provider = provider
            });

            ConfigureAuth(app);
        }
    }
}