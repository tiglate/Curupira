using Curupira.WindowsService;
using Microsoft.Owin.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Curupira.Tests.WindowsService
{
    [TestClass]
    public class ApiKeyValidationTests
    {
        private TestServer _server;
        private HttpClient _client;
        private string _validApiKey;

        [TestInitialize]
        public void Setup()
        {
            _validApiKey = Guid.NewGuid().ToString();
            Environment.SetEnvironmentVariable("API_KEY", _validApiKey);
            _server = TestServer.Create<Startup>();
            _client = _server.HttpClient;
        }

        [TestCleanup]
        public void Cleanup()
        {
            _client.Dispose();
            _server.Dispose();
        }

        [TestMethod]
        public async Task GetSystemUsage_WithValidApiKey_ReturnsOk()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/system/usage");
            request.Headers.Add("X-Api-Key", _validApiKey);

            // Act
            var response = await _client.SendAsync(request).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task GetSystemUsage_WithInvalidApiKey_ReturnsUnauthorized()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/system/usage");
            request.Headers.Add("X-Api-Key", "invalidApiKey");

            // Act
            var response = await _client.SendAsync(request).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task GetSystemUsage_WithNoApiKey_ReturnsUnauthorized()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/system/usage");

            // Act
            var response = await _client.SendAsync(request).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}