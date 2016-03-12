using System.Web.Mvc;
using SchedulerWebApp.Models.PostalEmail;

namespace SchedulerWebApp.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        
        public ActionResult LogOff()
        {
            return View();
        }
        

        public ActionResult FrequentlyAskedQuestions()
        {
            return View();
        }
        public ActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Contact([Bind(Include = "SenderFistName,SenderLastName,From,EmailSubject,EmailBody")]
            ContactUsEmail model)
        {
            if (!ModelState.IsValid) return View(model);
            
            model.To = "admin@schedule.com";
            PostalEmailManager.SendCorespondingEmail(model);
            return View("MessageSent");
        }
    }
}