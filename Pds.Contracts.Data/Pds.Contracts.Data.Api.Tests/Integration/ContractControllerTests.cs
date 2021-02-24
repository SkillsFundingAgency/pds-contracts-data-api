using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Pds.Contracts.Data.Common.Enums;
using Pds.Contracts.Data.Repository.Context;
using Pds.Contracts.Data.Services.Models;
using Pds.Contracts.Data.Services.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using DataModels = Pds.Contracts.Data.Repository.DataModels;

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
            appFactory = appFactory.WithWebHostBuilder(config =>
            {
                config.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(
                           d => d.ServiceType ==
                           typeof(DbContextOptions<PdsContext>));

                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    services.AddDbContext<PdsContext>(options =>
                        options.UseInMemoryDatabase("InMemory"));

                    // Build the service provider.
                    var serviceProvider = services.BuildServiceProvider();

                    // Create a scope to obtain a reference to the database context (PdsContext).
                    using var scope = serviceProvider.CreateScope();

                    var db = scope.ServiceProvider.GetRequiredService<PdsContext>();
                    List<DataModels.Contract> working = GetDataModelContracts();
                    if (db.Contracts.Count() <= 0)
                    {
                        foreach (var item in working)
                        {
                            db.Contracts.Add(item);
                        }

                        db.SaveChanges();
                    }
                });
            });

            _testClient = appFactory.CreateClient();
            _testClient.BaseAddress = new Uri("http://localhost:5001");
            _testClient.DefaultRequestHeaders.Accept.Clear();
            _testClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        [TestMethod]
        public async Task GetContractRemindersAsync_WithDefaultParameters_ReturnsResponse()
        {
            // Arrange

            // Act
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

        [TestMethod]
        public async Task UpdateLastEmailReminderSentAsync_WithDefaultParameters_ReturnsResponse()
        {
            // Arrange
            var content = new ContractRequest()
            {
                Id = 1,
                ContractNumber = "Levy-0002",
                ContractVersion = 1
            };

            // Act
            var response = await _testClient.PatchAsync("/api/contractReminder", GetStringContent(content));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task UpdateLastEmailReminderSentAsync_WithDefaultParameters_Returns400Response()
        {
            // Arrange
            var content = new ContractRequest()
            {
                Id = 0,
                ContractNumber = "Levy-0002",
                ContractVersion = 0
            };

            // Act
            var response = await _testClient.PatchAsync("/api/contractReminder", GetStringContent(content));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task UpdateLastEmailReminderSentAsync_WithDefaultParameters_Returns404Response()
        {
            // Arrange
            var content = new ContractRequest()
            {
                Id = 99,
                ContractNumber = "Levy-0002",
                ContractVersion = 1
            };

            // Act
            var response = await _testClient.PatchAsync("/api/contractReminder", GetStringContent(content));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task UpdateContractConfirmApproval_WithDefaultParameters_ReturnsResponse()
        {
            // Arrange
            var content = new UpdateConfirmApprovalRequest()
            {
                Id = 4,
                ContractNumber = "Levy-0002",
                ContractVersion = 1
            };

            // Act
            var response = await _testClient.PatchAsync("/api/confirmApproval", GetStringContent(content));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task UpdateContractConfirmApproval_WithDefaultParameters_Returns400Response()
        {
            // Arrange
            var content = new UpdateConfirmApprovalRequest()
            {
                Id = 0,
                ContractNumber = "Levy-0002",
                ContractVersion = 0
            };

            // Act
            var response = await _testClient.PatchAsync("/api/confirmApproval", GetStringContent(content));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task UpdateContractConfirmApproval_WithDefaultParameters_Returns404Response()
        {
            // Arrange
            var content = new UpdateConfirmApprovalRequest()
            {
                Id = 99,
                ContractNumber = "Levy-0002",
                ContractVersion = 1
            };

            // Act
            var response = await _testClient.PatchAsync("/api/confirmApproval", GetStringContent(content));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task UpdateContractWithdrawalAsync_WithDefaultParameters_ReturnsResponse()
        {
            // Arrange
            var content = new UpdateContractWithdrawalRequest()
            {
                Id = 7,
                ContractNumber = "Main-0001",
                ContractVersion = 1,
                WithdrawalType = ContractStatus.WithdrawnByAgency
            };

            // Act
            var response = await _testClient.PatchAsync("/api/withdraw", GetStringContent(content));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task UpdateContractWithdrawalAsync_WithDefaultParameters_Returns400Response()
        {
            // Arrange
            var content = new UpdateContractWithdrawalRequest()
            {
                Id = 0,
                ContractNumber = "Levy-0002",
                ContractVersion = 0,
                WithdrawalType = ContractStatus.WithdrawnByAgency
            };

            // Act
            var response = await _testClient.PatchAsync("/api/withdraw", GetStringContent(content));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task UpdateContractWithdrawalAsync_WithDefaultParameters_Returns404Response()
        {
            // Arrange
            var content = new UpdateContractWithdrawalRequest()
            {
                Id = 99,
                ContractNumber = "Levy-0002",
                ContractVersion = 1,
                WithdrawalType = ContractStatus.WithdrawnByAgency
            };

            // Act
            var response = await _testClient.PatchAsync("/api/withdraw", GetStringContent(content));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        private StringContent GetStringContent(object obj)
            => new StringContent(JsonConvert.SerializeObject(obj), Encoding.Default, "application/json");

        private List<DataModels.Contract> GetDataModelContracts()
        {
            string contractNumber = "Test-Contract-Number";
            string title = "Test Title";
            DateTime lastEmailReminderSent = DateTime.UtcNow;
            return new List<DataModels.Contract>
            {
                new DataModels.Contract { Id = 1, Title = title, ContractNumber = contractNumber, ContractVersion = 1, Ukprn = 12345678, CreatedAt = lastEmailReminderSent.AddDays(-45), LastEmailReminderSent = lastEmailReminderSent, Status = (int)ContractStatus.PublishedToProvider, FundingType = (int)ContractFundingType.AdvancedLearnerLoans },
                new DataModels.Contract { Id = 2, Title = title, ContractNumber = contractNumber, ContractVersion = 2, Ukprn = 12345678, CreatedAt = lastEmailReminderSent.AddDays(-45), LastEmailReminderSent = null, Status = (int)ContractStatus.PublishedToProvider, FundingType = (int)ContractFundingType.AdvancedLearnerLoans },
                new DataModels.Contract { Id = 3, Title = title, ContractNumber = contractNumber, ContractVersion = 3, Ukprn = 12345678, CreatedAt = lastEmailReminderSent.AddDays(-45), LastEmailReminderSent = null, Status = (int)ContractStatus.PublishedToProvider, FundingType = (int)ContractFundingType.AdvancedLearnerLoans },
                new DataModels.Contract { Id = 4, Title = title, ContractNumber = contractNumber, ContractVersion = 1, Ukprn = 12345678, CreatedAt = lastEmailReminderSent, LastEmailReminderSent = null, Status = (int)ContractStatus.ApprovedWaitingConfirmation, FundingType = (int)ContractFundingType.AdvancedLearnerLoans },
                new DataModels.Contract { Id = 5, Title = title, ContractNumber = contractNumber, ContractVersion = 1, Ukprn = 123456785, CreatedAt = lastEmailReminderSent, LastEmailReminderSent = null, Status = (int)ContractStatus.PublishedToProvider, FundingType = (int)ContractFundingType.AdvancedLearnerLoans },
                new DataModels.Contract { Id = 6, Title = title, ContractNumber = contractNumber, ContractVersion = 1, Ukprn = 123456786, CreatedAt = lastEmailReminderSent, LastEmailReminderSent = null, Status = (int)ContractStatus.ApprovedWaitingConfirmation, FundingType = (int)ContractFundingType.AdvancedLearnerLoans },
                new DataModels.Contract { Id = 7, Title = title, ContractNumber = contractNumber, ContractVersion = 1, Ukprn = 12345678, CreatedAt = lastEmailReminderSent, Status = (int)ContractStatus.PublishedToProvider },
                new DataModels.Contract { Id = 8, Title = title, ContractNumber = contractNumber, ContractVersion = 1, Ukprn = 12345678, CreatedAt = lastEmailReminderSent, Status = (int)ContractStatus.PublishedToProvider }
            };
        }
    }
}