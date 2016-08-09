using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SchedulerWebApp.Controllers;

namespace SchedulerWebApp.Tests.Controllers
{
    [TestClass]
    public class HomeControllerTest
    {
        [TestMethod]
        public void Index_Should_Return_View()
        {
            // Arrange
            /*var request = new Mock<HttpRequestBase>();
            request.SetupGet(x => x.IsAuthenticated).Returns(true);
            
            var context = new Mock<HttpContextBase>();
            context.SetupGet(x => x.Request).Returns(request.Object);

            var controller = new HomeController();
            controller.ControllerContext = new ControllerContext(context.Object, new RouteData(), controller);

            // Act
            var result = controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);*/
        }

        [TestMethod]
        public void Contact_Should_Return_View()
        {
            // Arrange
            var controller = new HomeController();

            // Act
            var result = controller.Contact() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }
    }
}
