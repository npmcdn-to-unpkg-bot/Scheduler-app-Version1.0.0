﻿using System;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace SchedulerWebApp.Models.DBContext
{
    public class CustomDbInitializer : DropCreateDatabaseIfModelChanges<SchedulerDbContext>
    {

        protected override void Seed(SchedulerDbContext context)
        {
            //User details
            var admin = new SchedulerUser
                        {
                            FirstName = "Administrator",
                            LastName = "Web Admin",
                            Email = "admin@scheduler.com",
                            UserName = "admin@scheduler.com"
                        };
            var firstUser = new SchedulerUser
                             {
                                 FirstName = "FirstUser-Name",
                                 LastName = "User",
                                 Email = "firstUser@gmail.com",
                                 UserName = "firstUser@gmail.com"
                             };

            var secondUser = new SchedulerUser
                             {
                                 FirstName = "SecondUser-Name",
                                 LastName = "User",
                                 Email = "secondUser@mail.com",
                                 UserName = "secondUser@mail.com"
                             };

            //some events
            var footballTournament = new Event
                              {
                                  Title = "Football tournament",
                                  Description = "Winter indoor tournament is around the conner. Please let us know if you will take part this year!",
                                  StartDate = new DateTime(2015, 03, 27),
                                  EndDate = new DateTime(2015, 03, 28),
                                  ReminderDate = new DateTime(2015, 03, 27),
                                  ListDate = new DateTime(2015, 03, 28),
                                  Location = "Some football hall"
                              };
            var newYearParty = new Event
                                {
                                    Title = "New year party",
                                    Description = "Please join us for new year part at my house",
                                    StartDate = new DateTime(2015, 12, 31),
                                    EndDate = new DateTime(2016, 01, 01),
                                    ReminderDate = new DateTime(2015, 12, 27),
                                    ListDate = new DateTime(2015, 12, 28),
                                    Location = "Street 123, City"
                                };

            var aspNetCoding = new Event
                               {
                                   Title = "ASP.NET Coding",
                                   Description = "Do you have knowledge or want to learn on asp.net? " +
                                                 "asp.net developers will be meeting to exchange ideas, " +
                                                 "Let us know if you will be joining us",
                                   StartDate = new DateTime(2015, 03, 20),
                                   EndDate = new DateTime(2015, 03, 20),
                                   ReminderDate = new DateTime(2015, 03, 10),
                                   ListDate = new DateTime(2015, 03, 13),
                                   Location = "Developers Hall"
                               };


            if (!context.Users.Any(u => u.UserName == "admin@scheduler.com"))
            {
                var userStore = new UserStore<SchedulerUser>(context);
                var userManager = new UserManager<SchedulerUser>(userStore);

                var roleStore = new RoleStore<IdentityRole>(context);
                var roleManager = new RoleManager<IdentityRole>(roleStore);

                userManager.Create(admin, "passW0rd!");
                roleManager.Create(new IdentityRole { Name = "Admin" });
                userManager.AddToRole(admin.Id, "Admin");
                userManager.AddClaim(admin.Id, new Claim(ClaimTypes.GivenName, admin.FirstName));
                
                userManager.Create(firstUser, "passW0rd!");
                userManager.AddClaim(firstUser.Id, new Claim(ClaimTypes.GivenName, firstUser.FirstName));

                userManager.Create(secondUser, "passW0rd!");
                userManager.AddClaim(secondUser.Id, new Claim(ClaimTypes.GivenName, secondUser.FirstName));
            }

            footballTournament.SchedulerUserId = firstUser.Id;
            newYearParty.SchedulerUserId = firstUser.Id;
            aspNetCoding.SchedulerUserId = secondUser.Id;

            context.Events.Add(newYearParty);
            context.Events.Add(footballTournament);
            context.Events.Add(aspNetCoding);

            context.SaveChanges();

            base.Seed(context);
        }
    }
}