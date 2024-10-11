﻿using Curupira.WindowsService;
using Microsoft.Owin.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Curupira.Tests.WindowsService
{
    [TestClass]
    public class StartupIntegrationTests
    {
        private TestServer _server;
        private HttpClient _client;

        [TestInitialize]
        public void Setup()
        {
            Environment.SetEnvironmentVariable("API_KEY", Guid.NewGuid().ToString());
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
        public async Task Get_Root_ReturnsNotFound()
        {
            // Act
            var response = await _client.GetAsync("/").ConfigureAwait(false);

            // Assert
            Assert.AreEqual(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Get_Api_ReturnsOk()
        {
            // Act
            var response = await _client.GetAsync("/api/health").ConfigureAwait(false);

            // Assert
            Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void Startup_ThrowsException_WhenApiKeyNotDefined()
        {
            // Arrange
            Environment.SetEnvironmentVariable("API_KEY", null);

            using (var server = TestServer.Create<Startup>())
            {
                // Act & Assert
                Assert.ThrowsException<InvalidOperationException>(() => server.HttpClient.GetAsync("/api/health").RunSynchronously());
            }
        }
    }
}
