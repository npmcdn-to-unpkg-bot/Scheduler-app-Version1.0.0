﻿using System.Web.Mvc;
using SchedulerWebApp.Models.PostalEmail;
using SchedulerWebApp.Models.Service;
using SchedulerWebApp.Models.ViewModels;

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
        public ActionResult Contact([Bind(Include = "SenderFistName,SenderLastName,SenderEmail,EmailSubject,EmailBody")]
            ContactUsEmail model)
        {
            if (ModelState.IsValid)
            {
                //var emailDelivery = new EmailDelivery();
                //emailDelivery.SendContactFormulaEmail(model, "ContactFormulaEmail");

                model.ReceiverEmail = "admin@schedule.com";
                PostalEmailManager.SendContactUsEmail(model);

                return View("MessageSent");
            }
            return View(model);
        }
    }
}