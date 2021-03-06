﻿using System.Web;
using System.Web.Mvc;
using SchedulerWebApp.Models;
using SchedulerWebApp.Models.PostalEmail;

namespace SchedulerWebApp.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly Service _service = new Service();

        public ActionResult Index()
        {
            if (!Request.IsAuthenticated)
            {
                return View("IndexNotAuthenticated");
            }

            var user = _service.GetUser();

           return View("Index", user);
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
            return RedirectToAction("Index");
        }
    }
}