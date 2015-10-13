using System.Net;
using System.Net.Http;
using HealthMonitoring.SelfHost.Controllers;
using Xunit;

namespace HealthMonitoring.UnitTests.SelfHost.Controllers
{
    public class StaticControllerTests
    {
        [Fact]
        public void GetHome_should_return_home_page()
        {
            var controller = new StaticController();
            var response = controller.GetHome();
            AssertValidFile(response, "text/html");
        }

        [Fact]
        public void GetDashboard_should_return_home_page()
        {
            var controller = new StaticController();
            var response = controller.GetDashboard();
            AssertValidFile(response, "text/html");
        }

        [Theory]
        [InlineData("favicon.ico", "image/x-icon")]
        [InlineData("main.css", "text/css")]
        [InlineData("angular.min.js", "application/javascript")]
        public void GetStatic_should_return_content(string path, string mediaType)
        {
            var controller = new StaticController();
            var response = controller.GetStatic(path);
            AssertValidFile(response, mediaType);
        }

        [Fact]
        public void GetStatic_should_return_404_for_not_known_files()
        {
            var controller = new StaticController();
            var response = controller.GetStatic("something.png");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        private static void AssertValidFile(HttpResponseMessage response, string mediaType)
        {
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotEmpty(response.Content.ReadAsByteArrayAsync().Result);
            Assert.Equal(mediaType, response.Content.Headers.ContentType.MediaType);
        }
    }
}