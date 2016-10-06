using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using HealthMonitoring.SelfHost.Configuration;
using Moq;
using Xunit;

namespace HealthMonitoring.Api.UnitTests.Formatters
{
    public class TextMediaTypeFormatterTests
    {
        [Fact]
        public void TextMediaTypeFormatter_AddsSupportForPlainText()
        {
            var mediaTypeFormatter = new TextMediaTypeFormatter();

            Assert.NotNull(mediaTypeFormatter.SupportedMediaTypes.FirstOrDefault(x => x.MediaType == "text/xml"));
            Assert.NotNull(mediaTypeFormatter.SupportedMediaTypes.FirstOrDefault(x => x.MediaType == "text/plain"));
            Assert.NotNull(mediaTypeFormatter.SupportedMediaTypes.FirstOrDefault(x => x.MediaType == "text/javascript"));
        }

        [Fact]
        public void TextMediaTypeFormatter_ReadsTypesTextAndNotOtherTypes()
        {
            var mediaTypeFormatter = new TextMediaTypeFormatter();

            Assert.True(mediaTypeFormatter.CanReadType(typeof(string)));
            Assert.False(mediaTypeFormatter.CanReadType(typeof(int)));
        }

        [Fact]
        public void TextMediaTypeFormatter_CannotWriteAnyType()
        {
            var mediaTypeFormatter = new TextMediaTypeFormatter();

            Assert.False(mediaTypeFormatter.CanWriteType(typeof(string)));
            Assert.False(mediaTypeFormatter.CanWriteType(typeof(int)));
        }

        [Theory]
        [InlineData("UTF-16")]
        [InlineData("UTF-8")]
        public void TextMediaTypeFormatter_CanReadTextStream(string encoding)
        {
            var mediaTypeFormatter = new TextMediaTypeFormatter();
            var expected = "this is my text £!";
            HttpContent content = new StringContent(expected, Encoding.GetEncoding(encoding));

            var formatterLogger = new Mock<IFormatterLogger>();
            var result = mediaTypeFormatter.ReadFromStreamAsync(typeof(string), content.ReadAsStreamAsync().Result, content, formatterLogger.Object);

            Assert.Equal(result.Result.ToString(), expected);
        }
    }
}
