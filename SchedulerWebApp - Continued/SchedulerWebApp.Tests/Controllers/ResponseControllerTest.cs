using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SchedulerWebApp.Controllers;
using SchedulerWebApp.Models;
using SchedulerWebApp.Models.DBContext;
using SchedulerWebApp.Models.ViewModels;

namespace SchedulerWebApp.Tests.Controllers
{
    [TestClass]
    public class ResponseControllerTest
    {
        [TestMethod]
        public void Response_Should_Return_View_OfType_ResponseViewModel()
        {
            //arrange
            var mockContext = new Mock<SchedulerDbContext>();
            mockContext.Setup(c => c.Events.Find(1)).Returns(new Event { Id = 1, Participants = new List<Participant> { new Participant { Id = 1 } } });
            var controller = new ResponseController(mockContext.Object);

            //act
            var result = controller.Response(1, 1) as ViewResult;

            //Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.ViewData.Model, typeof(ResponseViewModel));
        }

        [TestMethod]
        public void Response_Should_Return_HttpNotfound()
        {
            //arrange 
            var mockContext = new Mock<SchedulerDbContext>();
            mockContext.Setup(c => c.Events.Find(1)).Returns(new Event { Id = 1, Participants = new List<Participant> { new Participant { Id = 1 } } });
            var controller = new ResponseController(mockContext.Object);

            //act
            var result = controller.Response(1, 22) as HttpNotFoundResult;

            //Assert
            Assert.AreEqual(404, result.StatusCode);
        }

        [TestMethod]
        public void Response_Should_Return_CantRespond_View()
        {
            var mockContext = new Mock<SchedulerDbContext>();
            mockContext.Setup(c => c.Events.Find(1)).Returns(new Event
                                                             {
                                                                 Id = 1,
                                                                 ListDate = new DateTime(2015, 02, 10),
                                                                 Participants = new List<Participant> { new Participant { Id = 1 } }
                                                             });
            var controller = new ResponseController(mockContext.Object);

            //act
            var result = controller.Response(new ResponseViewModel { EventId = 1 }) as ViewResult;

            //assert
            Assert.AreEqual("_CantRespond", result.ViewName);
        }

        [TestMethod]
        public void Response_Should_Return_Model_OfType_ResponseViewModel()
        {
            //arrange
            var mockContext = new Mock<SchedulerDbContext>();
            mockContext.Setup(c => c.Events.Find(1)).Returns(new Event
                                                             {
                                                                 Id = 1,
                                                                 ListDate = new DateTime(2015, 04, 10),
                                                                 Participants = null
                                                             });
            var controller = new ResponseController(mockContext.Object);

            //act
            var result = controller.Response(new ResponseViewModel { EventId = 1 ,ParticipantId = 10}) as ViewResult;

            //Assert
            Assert.IsInstanceOfType(result.ViewData.Model,typeof(ResponseViewModel));
        }

        [TestMethod]
        public void Details_Should_Return_HttpNotFound()
        {
            //arrange
            var mockContext = new Mock<SchedulerDbContext>();
            var controller = new ResponseController(mockContext.Object);

            //act
            var result = controller.Details(null) as HttpNotFoundResult;

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(404, result.StatusCode);
        }
    }
}
