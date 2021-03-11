using Azure.Storage.Blobs;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Pds.Contracts.Data.Common.Enums;
using Pds.Contracts.Data.Repository.Context;
using Pds.Contracts.Data.Services.Models;
using Pds.Contracts.Data.Services.Responses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using DataModels = Pds.Contracts.Data.Repository.DataModels;

namespace Pds.Contracts.Data.Api.Tests.Integration
{
    [TestClass]
    [TestCategory("Integration")]
    public class ContractControllerTests
    {
        private readonly HttpClient _testClient;

        private readonly string _manualApproveUrl = "/api/Contract/manualApprove";

        private readonly string _blobName = "sample-blob-file.xml";

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

        [TestInitialize]
        public void TestInitiaize()
        {
            CreateSampleBlobFile();
        }

        #region Create Tests

        [TestMethod]
        public async Task Create_WithTestContract_CreatesContractInSystemTest()
        {
            // Arrange
            CreateContractRequest request = Generate_CreateContractRequest();

            // Act
            var response = await _testClient.PostAsync("/api/contract", GetStringContent(request));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [TestMethod]
        public async Task Create_InvalidContractNumber_ReturnsBadResponseTest()
        {
            // Arrange
            CreateContractRequest request = Generate_CreateContractRequest();
            request.ContractNumber = string.Empty;

            // Act
            var response = await _testClient.PostAsync("/api/contract", GetStringContent(request));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Create_InvalidContractVersion_ReturnsBadResponseTest()
        {
            // Arrange
            CreateContractRequest request = Generate_CreateContractRequest();
            request.ContractVersion = -1;

            // Act
            var response = await _testClient.PostAsync("/api/contract", GetStringContent(request));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Create_DuplicateContractCreateRequest_ReturnsConflictResponseTest()
        {
            // Arrange
            CreateContractRequest request = Generate_CreateContractRequest();
            request.ContractNumber = "Test-Contract-Number";
            request.ContractVersion = 3;

            // Act
            var response = await _testClient.PostAsync("/api/contract", GetStringContent(request));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        [TestMethod]
        public async Task Create_ContractCreateRequest_WhenContractWithHigherVersionExists_ReturnsPreconditionFailedResponseTest()
        {
            // Arrange
            CreateContractRequest request = Generate_CreateContractRequest();
            request.ContractNumber = "Test-Contract-High";
            request.ContractVersion = 2;

            // Act
            var response = await _testClient.PostAsync("/api/contract", GetStringContent(request));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.PreconditionFailed);
        }

        #endregion


        #region GetContractReminder

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

        #endregion


        #region UpdateLastEmailReminder

        [TestMethod]
        public async Task UpdateLastEmailReminderSentAsync_WithDefaultParameters_ReturnsResponse()
        {
            // Arrange
            var content = new ContractRequest()
            {
                Id = 1,
                ContractNumber = "Levy-0002",
                ContractVersion = 1,
                FileName = _blobName
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
                ContractVersion = 0,
                FileName = _blobName
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
                ContractVersion = 1,
                FileName = _blobName
            };

            // Act
            var response = await _testClient.PatchAsync("/api/contractReminder", GetStringContent(content));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        #endregion


        #region UpdateContractConfirmApproval

        [TestMethod]
        public async Task UpdateContractConfirmApproval_WithDefaultParameters_ReturnsResponse()
        {
            // Arrange
            var content = new UpdateConfirmApprovalRequest()
            {
                Id = 4,
                ContractNumber = "Test-Contract-Number",
                ContractVersion = 1,
                FileName = _blobName
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
                ContractVersion = 0,
                FileName = _blobName
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
                ContractVersion = 1,
                FileName = _blobName
            };

            // Act
            var response = await _testClient.PatchAsync("/api/confirmApproval", GetStringContent(content));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        #endregion UpdateContractConfirmApproval


        #region ManualApprove tests.

        [TestMethod]
        public async Task ManualApprove_WithValidInput_ReturnsResponse()
        {
            // Arrange
            var content = GetContractRequest();

            // Act
            var response = await _testClient.PatchAsync(_manualApproveUrl, GetStringContent(content));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task ManualApprove_WithInvalidInput_Returns400Response()
        {
            // Arrange
            var content = GetContractRequest();
            content.Id = 0;

            // Act
            var response = await _testClient.PatchAsync(_manualApproveUrl, GetStringContent(content));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task ManualApprove_WithValidInput_Returns404Response()
        {
            // Arrange
            var content = GetContractRequest();
            content.Id = 99;

            // Act
            var response = await _testClient.PatchAsync(_manualApproveUrl, GetStringContent(content));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        #endregion


        #region ManualApprove tests.

        [TestMethod]
        public async Task ManualApprove_ContractExpectationFailedException_Returns404Response()
        {
            // Arrange
            var content = GetContractRequest();
            content.Id = 1;

            // Act
            var response = await _testClient.PatchAsync(_manualApproveUrl, GetStringContent(content));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task ManualApprove_WithValidInput_Returns412Response()
        {
            // Arrange
            var content = GetContractRequest();
            content.Id = 6;

            // Act
            var response = await _testClient.PatchAsync(_manualApproveUrl, GetStringContent(content));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.PreconditionFailed);
        }

        #endregion ManualApprove tests.


        #region WithdrawalAsync
        [TestMethod]
        public async Task WithdrawalAsync_WithContractStatusAsString_ReturnsResponse()
        {
            // Arrange
            var options = new JsonWriterOptions
            {
                Indented = true
            };
            string json = null;
            using (var stream = new MemoryStream())
            {
                using (var writer = new Utf8JsonWriter(stream, options))
                {
                    writer.WriteStartObject();
                    writer.WriteNumber("id", 10);
                    writer.WriteString("contractNumber", "Test-Contract-High");
                    writer.WriteNumber("contractVersion", 1);
                    writer.WriteString("withdrawalType", "WithdrawnByAgency");
                    writer.WriteString("fileName", _blobName);
                    writer.WriteEndObject();
                }

                json = Encoding.UTF8.GetString(stream.ToArray());
                Console.WriteLine(json);
            }

            StringContent content = new StringContent(json, Encoding.Default, "application/json");

            // Act
            var response = await _testClient.PatchAsync("/api/withdraw", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task WithdrawalAsync_WithDefaultParameters_ReturnsResponse()
        {
            // Arrange
            var content = new UpdateContractWithdrawalRequest()
            {
                Id = 7,
                ContractNumber = "Test-Contract-Number",
                ContractVersion = 1,
                WithdrawalType = ContractStatus.WithdrawnByAgency,
                FileName = _blobName
            };

            // Act
            var response = await _testClient.PatchAsync("/api/withdraw", GetStringContent(content));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task WithdrawalAsync_WithDefaultParameters_Returns400Response()
        {
            // Arrange
            var content = new UpdateContractWithdrawalRequest()
            {
                Id = 0,
                ContractNumber = "Levy-0002",
                ContractVersion = 0,
                WithdrawalType = ContractStatus.WithdrawnByAgency,
                FileName = _blobName
            };

            // Act
            var response = await _testClient.PatchAsync("/api/withdraw", GetStringContent(content));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task WithdrawalAsync_WithDefaultParameters_Returns404Response()
        {
            // Arrange
            var content = new UpdateContractWithdrawalRequest()
            {
                Id = 99,
                ContractNumber = "Levy-0002",
                ContractVersion = 1,
                WithdrawalType = ContractStatus.WithdrawnByAgency,
                FileName = _blobName
            };

            // Act
            var response = await _testClient.PatchAsync("/api/withdraw", GetStringContent(content));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        #endregion

        private StringContent GetStringContent(object obj)
            => new StringContent(System.Text.Json.JsonSerializer.Serialize(obj, new JsonSerializerOptions() { Converters = { new JsonStringEnumConverter() } }), Encoding.Default, "application/json");

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
                new DataModels.Contract { Id = 5, Title = title, ContractNumber = contractNumber, ContractVersion = 1, Ukprn = 123456785, CreatedAt = lastEmailReminderSent, LastEmailReminderSent = null, Status = (int)ContractStatus.PublishedToProvider, FundingType = (int)ContractFundingType.AdvancedLearnerLoans, ContractContent = GetDataModelContractContent(5) },
                new DataModels.Contract { Id = 6, Title = title, ContractNumber = contractNumber, ContractVersion = 1, Ukprn = 123456786, CreatedAt = lastEmailReminderSent, LastEmailReminderSent = null, Status = (int)ContractStatus.ApprovedWaitingConfirmation, FundingType = (int)ContractFundingType.AdvancedLearnerLoans, ContractContent = GetDataModelContractContent(6) },
                new DataModels.Contract { Id = 7, Title = title, ContractNumber = contractNumber, ContractVersion = 1, Ukprn = 12345678, CreatedAt = lastEmailReminderSent, Status = (int)ContractStatus.PublishedToProvider },
                new DataModels.Contract { Id = 8, Title = title, ContractNumber = contractNumber, ContractVersion = 1, Ukprn = 12345678, CreatedAt = lastEmailReminderSent, Status = (int)ContractStatus.PublishedToProvider },
                new DataModels.Contract { Id = 9, Title = title, ContractNumber = "Test-Contract-High", ContractVersion = 10, Ukprn = 12345678, CreatedAt = lastEmailReminderSent, LastEmailReminderSent = null, Status = (int)ContractStatus.ApprovedWaitingConfirmation, FundingType = (int)ContractFundingType.AdvancedLearnerLoans },
                new DataModels.Contract { Id = 10, Title = title, ContractNumber = "Test-Contract-High", ContractVersion = 1, Ukprn = 12345678, CreatedAt = lastEmailReminderSent, Status = (int)ContractStatus.PublishedToProvider, ContractData = new DataModels.ContractData() { Id = 10, OriginalContractXml = "<contract>sample.xml.data</contract>" } }
            };
        }

        private CreateContractRequest Generate_CreateContractRequest()
        {
            DateTime startDate = new DateTime(2021, 4, 1);
            startDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);

            DateTime endDate = new DateTime(2022, 3, 31);
            endDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);

            var request = new CreateContractRequest()
            {
                UKPRN = 12345678,
                Title = "Test contract title",
                ContractNumber = "Test123",
                ContractVersion = 1,
                Value = 1000m,
                FundingType = ContractFundingType.Unknown,
                Year = "2021",
                Type = ContractType.ConditionsOfFundingGrant,
                ParentContractNumber = null,
                StartDate = startDate,
                EndDate = endDate,
                AmendmentType = ContractAmendmentType.None,
                ContractAllocationNumber = null,
                FirstCensusDateId = null,
                SecondCensusDateId = null,
                CreatedBy = "Feed",
                ContractContent = new CreateContractRequestDocument()
                {
                    Content = new byte[20],
                    Size = 20,
                    FileName = "Test file"
                },
                PageCount = 0,
                ContractData = _blobName,
                ContractFundingStreamPeriodCodes = new CreateContractCode[] { new CreateContractCode() { Code = "Test" } }
            };
            return request;
        }

        private DataModels.ContractContent GetDataModelContractContent(int id)
        {
            var pdfContent = GetPdfContent();
            return new DataModels.ContractContent()
            {
                Id = id,
                FileName = "sdf",
                Content = pdfContent,
                Size = pdfContent.Length
            };
        }

        private byte[] GetPdfContent()
        {
            var pdfResources = "Pds.Contracts.Data.Api.Tests.Integration.Resources.test.pdf";
            using (Stream resFilestream = typeof(ContractControllerTests).Assembly.GetManifestResourceStream(pdfResources))
            {
                byte[] ba = new byte[resFilestream.Length];
                resFilestream.Read(ba, 0, ba.Length);
                return ba;
            }
        }

        private ContractRequest GetContractRequest()
        {
            return new ContractRequest()
            {
                Id = 5,
                ContractNumber = "Test-Contract-Number",
                ContractVersion = 1,
                FileName = _blobName
            };
        }

        private void CreateSampleBlobFile()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.development.json", optional: false, reloadOnChange: true)
                .Build();
            var sampleBlobFileContent = "<contract xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns='urn:sfa:schemas:contract'></contract>";
            BlobContainerClient containerClient = new BlobContainerClient(configuration.GetSection("AzureBlobConfiguration").GetSection("ConnectionString").Value, "contractevents");
            containerClient.CreateIfNotExists();
            UTF8Encoding encoding = new UTF8Encoding();
            using MemoryStream memoryStream = new MemoryStream(encoding.GetBytes(sampleBlobFileContent));
            containerClient.DeleteBlobIfExists(_blobName);
            containerClient.UploadBlob(_blobName, memoryStream);
        }
    }
}