using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SchedulerWebApp.Controllers;
using SchedulerWebApp.Models;
using SchedulerWebApp.Models.DBContext;

namespace SchedulerWebApp.Tests.Controllers
{
    [TestClass]
    public class EventsControllerTest
    {
        [TestMethod]
        public void Create_Should_Return_Create_View()
        {
            //Arrange
            var mockSet = new Mock<DbSet<Event>>();
            var mockContext = new Mock<SchedulerDbContext>();
            mockContext.Setup(c => c.Events).Returns(mockSet.Object);

            var controller = new EventsController();

            //Act
            var result = controller.Create() as ViewResult;

            //Assert
            Assert.IsNotNull(result);
        }

        /*[TestMethod]
        public void Create_Should_Redirect_To_SendInvitation_In_Invitation_Controller()
        {
            //arrange
            var controller = Arrange_Event_Controller();

            //act 
            var result = controller.Create(new Event()) as RedirectToRouteResult;

            //Assert
            Assert.AreEqual("SendEventsInvitation", result.RouteValues["Action"]);
            Assert.AreEqual("Invitation", result.RouteValues["Controller"]);
        }*/

        /*[TestMethod]
        public void GetUserEvents_Should_Get_All_User_Events()
        {
            //arrange
            var controller = Arrange_Event_Controller();

            //act
            var result = controller.GetUserEvents();

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        public void Edit_Should_Return_View_For_Event()
        {
            //arrange
            var controller = Arrange_Event_Controller();
            
            //act
            var result = controller.Edit(1) as ViewResult;

            //assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.ViewData.Model,typeof(Event));
        }*/
        
        /*[TestMethod]
        public void Edit_Should_Redirect_To_Index()
        {
            var controller = Arrange_Event_Controller();

            var result = controller.Edit(new Event
                                         {
                                             Title = "test", Description = "D test", Id = 1, Location = "L Test",
                                             StartDate = new DateTime(2015,04,10), ListDate = new DateTime(2015,04,10),
                                             EndDate = new DateTime(2015, 04, 10), ReminderDate = new DateTime(2015,04,10),
                                             SchedulerUserId = "99fd20b8-7194-31e1-945b-e6736c732499",
                                             Participants = new List<Participant> { new Participant { Id = 1} }
                                         }, 1) as RedirectToRouteResult;

            Assert.IsNotNull(result);
        }*/

        public ControllerContext MockContext()
        {
            var claim = new Claim("testClaim", "99fd20b8-7194-31e1-945b-e6736c732499");
            var mockIdentity = Mock.Of<ClaimsIdentity>(ci => ci.FindFirst(It.IsAny<string>()) == claim);
            var mockPrincipal = Mock.Of<IPrincipal>(ip => ip.Identity == mockIdentity);
            var mockContext = Mock.Of<ControllerContext>(cc => cc.HttpContext.User == mockPrincipal);
            return mockContext;
        }

        public EventsController Arrange_Event_Controller()
        {
            var testEvent1 = new Event{Id = 1};
            var testEvent2 = new Event{Id = 2};

            var testUser = new SchedulerUser{Id = "99fd20b8-7194-31e1-945b-e6736c732499",Events = new List<Event> { testEvent1, testEvent2 }};

            var users = new List<SchedulerUser> { testUser }.AsQueryable();

            var mockSet = new Mock<DbSet<SchedulerUser>>();
            mockSet.As<IQueryable<SchedulerUser>>().Setup(m => m.Provider).Returns(users.Provider);
            mockSet.As<IQueryable<SchedulerUser>>().Setup(m => m.Expression).Returns(users.Expression);
            mockSet.As<IQueryable<SchedulerUser>>().Setup(m => m.ElementType).Returns(users.ElementType);
            mockSet.As<IQueryable<SchedulerUser>>().Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator);

            var context = new Mock<SchedulerDbContext>();
            context.Setup(c => c.Users).Returns(mockSet.Object);

            var controller = new EventsController(context.Object) { ControllerContext = MockContext() };

            return controller;
        }


    }
}
