using System.Net;
using HackerNewsBackend.Domain.Models;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using Xunit;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HackerNewsBackend.Services.Tests
{
    public class HackerNewsBackendServicesTests
    {
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly HackerNewsBackendServices _service;

        public HackerNewsBackendServicesTests()
        {
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockConfiguration = new Mock<IConfiguration>();
            _service = new HackerNewsBackendServices(_mockHttpClientFactory.Object, _mockConfiguration.Object);
        }

        [Fact]
        public async Task GetListTopStoriesAsync_ShouldReturnTopStories()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync((HttpRequestMessage request, CancellationToken cancellationToken) =>
                {
                    var response = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(JsonSerializer.Serialize(new List<int> { 1, 2, 3 }))
                    };

                    if (request.RequestUri.ToString().Contains("item"))
                    {
                        var story = new TopStories
                        {
                            Id = 1,
                            Title = "Test Story"
                        };
                        response.Content = new StringContent(JsonSerializer.Serialize(story));
                    }

                    return response;
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            _mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

            // Act
            var result = await _service.GetListTopStoriesAsync(1, 3);

            // Assert
            Assert.IsNotNull(result);
        }
    }
}