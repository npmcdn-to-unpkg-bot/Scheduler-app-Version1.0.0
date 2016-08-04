using System.Web;
using System.Web.Mvc;
using SchedulerWebApp.Models.PostalEmail;

namespace SchedulerWebApp.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (!Request.IsAuthenticated)
            {
                return View("IndexNotAuthenticated");
            }

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
            
            model.To = "s.buchumi@gmail.com";
            PostalEmailManager.SendContactUsEmail(model);

            Response.Cookies.Add(new HttpCookie("successCookie", "Action is completed successfully"));

            //remove MessageSent view fom home
            return View("Index");
        }
    }
}