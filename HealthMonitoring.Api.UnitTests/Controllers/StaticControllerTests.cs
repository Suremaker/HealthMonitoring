using System.IO;
using System.Net;
using System.Net.Http;
using HealthMonitoring.SelfHost.Controllers;
using Moq;
using Moq.Protected;
using Xunit;

namespace HealthMonitoring.Api.UnitTests.Controllers
{
    public class StaticControllerTests
    {
        [Fact]
        public void GetHome_should_return_home_page()
        {
            var controller = CreateController();
            var response = controller.GetHome();
            AssertValidFile(response, "text/html");
        }

        [Fact]
        public void GetDashboard_should_return_dashboard_page()
        {
            var controller = CreateController();
            var response = controller.GetDashboard();
            AssertValidFile(response, "text/html");
        }

        [Fact]
        public void GetEndpointDetails_should_return_details_page()
        {
            var controller = CreateController();
            var response = controller.GetEndpointDetails();
            AssertValidFile(response, "text/html");
        }

        [Theory]
        [InlineData("assets", "favicon.ico", "image/x-icon")]
        [InlineData("styles", "dashboard.css", "text/css")]
        [InlineData("scripts", "angular.min.js", "application/javascript")]
        [InlineData("assets", "favicon.svg", "image/svg+xml")]
        public void GetStatic_should_return_content(string directory, string file, string mediaType)
        {
            var controller = CreateController();
            var response = controller.GetStatic(directory, file);
            AssertValidFile(response, mediaType);
        }

        [Fact]
        public void GetStatic_should_return_content_with_static_ETag_and_NotModified_status_code()
        {
            var controller = CreateController();
            var response1 = controller.GetStatic("assets", "favicon.ico");
            Assert.NotEmpty(response1.Headers.ETag.Tag);

            controller.Request.Headers.IfNoneMatch.Add(response1.Headers.ETag);
            var response2 = controller.GetStatic("assets", "favicon.ico");

            Assert.Equal(HttpStatusCode.NotModified, response2.StatusCode);
        }

        [Theory]
        [InlineData("assets", "image.gif", "image/gif")]
        [InlineData("assets", "image.png", "image/png")]
        [InlineData("assets", "image.jpg", "image/jpeg")]
        [InlineData("assets", "image.jpeg", "image/jpeg")]
        public void GetStatic_should_return_custom_content(string directory, string path, string mediaType)
        {
            var controller = new Mock<StaticController>();
            controller.Protected().Setup<Stream>("GetCustomStream", directory, path).Returns(new MemoryStream(new[] { (byte)1 }));
            controller.Object.Request = new HttpRequestMessage();
            var response = controller.Object.GetStatic(directory, path);
            AssertValidFile(response, mediaType);
        }

        [Fact]
        public void GetStatic_should_return_404_for_not_known_files()
        {
            var controller = CreateController();
            var response = controller.GetStatic("assets", "something.png");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        private static void AssertValidFile(HttpResponseMessage response, string mediaType)
        {
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotEmpty(response.Content.ReadAsByteArrayAsync().Result);
            Assert.Equal(mediaType, response.Content.Headers.ContentType.MediaType);
        }

        private static StaticController CreateController()
        {
            return new StaticController { Request = new HttpRequestMessage() };
        }
    }
}