using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Pds.Contracts.Data.Services.Models;
using Pds.Contracts.Data.Services.Responses;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Pds.Contracts.Data.Api.Tests.Integration
{
    [TestClass]
    [TestCategory("Integration")]
    public class ContractControllerTests
    {
        private readonly HttpClient _testClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractControllerTests"/> class.
        /// </summary>
        public ContractControllerTests()
        {
            var appFactory = new WebApplicationFactory<Startup>();
            _testClient = appFactory.CreateClient();
            _testClient.BaseAddress = new Uri("http://localhost:5001");
            _testClient.DefaultRequestHeaders.Accept.Clear();
            _testClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        [TestMethod]
        public async Task GetContractRemindersAsync_WithDefaultParameters_ReturnsResponse()
        {
            // Arrange Act
            var response = await _testClient.GetAsync("/api/contractReminders");

            // Assert
            string contractsString = await response.Content.ReadAsStringAsync();
            var contracts = JsonConvert.DeserializeObject<ContractReminderResponse<IEnumerable<ContractReminderItem>>>(contractsString);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            contracts.Contracts.Should().NotBeNull();
            contracts.Paging.Should().NotBeNull();
        }

        [TestMethod]
        public async Task GetContractRemindersAsync_WithParameters_ReturnsResponse()
        {
            // Arrange
            int count = 2;
            int expectedCount = count;
            string reminderInterval = $"?reminderInterval=14&page=1&count={count}";

            // Act
            var response = await _testClient.GetAsync("/api/contractReminders" + reminderInterval);

            // Assert
            string contractsString = await response.Content.ReadAsStringAsync();
            ContractReminderResponse<IEnumerable<ContractReminderItem>> contracts = JsonConvert.DeserializeObject<ContractReminderResponse<IEnumerable<ContractReminderItem>>>(contractsString);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            contracts.Contracts.Should().HaveCount(expectedCount);
            contracts.Paging.Should().NotBeNull();
        }

        [TestMethod]
        public async Task GetContractRemindersAsync_WithParameters_ReturnsNotFound()
        {
            // Arrange
            int count = 2;
            int reminderInterval = 14;
            int pageNumber = 1000;
            string url = $"?reminderInterval={reminderInterval}&page={pageNumber}&count={count}";

            // Act
            var response = await _testClient.GetAsync("/api/contractReminders" + url);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task GetContractRemindersAsync_WithParameters_ReturnsNoContent()
        {
            // Arrange
            int count = 2;
            int reminderInterval = 500;
            int pageNumber = 1;
            string url = $"?reminderInterval={reminderInterval}&page={pageNumber}&count={count}";

            // Act
            var response = await _testClient.GetAsync("/api/contractReminders" + url);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }
    }
}