﻿using AutoMapper;
using FluentAssertions;
using MediatR;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Audit.Api.Client.Interfaces;
using Pds.Audit.Api.Client.Registrations;
using Pds.Contracts.Data.Common.Enums;
using Pds.Contracts.Data.Repository.Implementations;
using Pds.Contracts.Data.Services.AutoMapperProfiles;
using Pds.Contracts.Data.Services.DependencyInjection;
using Pds.Contracts.Data.Services.DocumentServices;
using Pds.Contracts.Data.Services.Implementations;
using Pds.Contracts.Data.Services.Interfaces;
using Pds.Contracts.Data.Services.Models;
using Pds.Contracts.Data.Services.Responses;
using Pds.Contracts.Data.Services.Tests.Integration.DocumentServices;
using Pds.Contracts.Data.Services.Tests.SetUp;
using Pds.Core.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AuditModels = Pds.Audit.Api.Client.Models;
using DataModels = Pds.Contracts.Data.Repository.DataModels;

namespace Pds.Contracts.Data.Services.Tests.Integration
{
    [TestClass, TestCategory("Integration")]
    public class ContractServiceTests
    {
        private readonly ISemaphoreOnEntity<string> _semaphoreOnEntity
            = new SemaphoreOnEntity<string>();

        private readonly Uri blobUri = new Uri("https://www.blob-container.com/");

        private Mock<IAuditService> _mockAuditService;

        private IMapper _mapper = null;


        [TestInitialize]
        public void TestInitiaize()
        {
            // Creates a sample-blob-file.xml file in blob storage for integration testing purposes.
            // Set your file name property to BlobHelper.BlobName
            BlobHelper.CreateSampleBlobFile();
        }

        #region Create

        [TestMethod]
        public async Task CreateAsync_WhenInputIsValid_Then_CreatesContractSuccessfullyTest()
        {
            // Arrange
            string baseUrl = $"https://localhost:5001";

            SetMapperHelper();
            var request = Generate_CreateContractRequest();

            ILoggerAdapter<ContractService> logger = new LoggerAdapter<ContractService>(new Logger<ContractService>(new LoggerFactory()));
            ILoggerAdapter<ContractRepository> loggerRepo = new LoggerAdapter<ContractRepository>(new Logger<ContractRepository>(new LoggerFactory()));

            MockAuditService();

            var inMemPdsDbContext = HelperExtensions.GetInMemoryPdsDbContext();
            var repo = new Repository<DataModels.Contract>(inMemPdsDbContext);
            var work = new SingleUnitOfWorkForRepositories(inMemPdsDbContext);
            var contractRepo = new ContractRepository(repo, work, loggerRepo);
            var uriService = new UriService(baseUrl);
            var asposeDocumentManagementContractService = GetDocumentService();
            var contractValidationService = GetContractValidationService();
            var mediator = BuildMediator();
            var contractDocumentService = BuildContractDocumentService();
            var service = new ContractService(contractRepo, _mapper, uriService, logger, _mockAuditService.Object, _semaphoreOnEntity, asposeDocumentManagementContractService, contractValidationService, mediator, contractDocumentService);

            // Act
            var before = await contractRepo.GetByContractNumberAsync(request.ContractNumber);

            await service.CreateAsync(request);

            var after = await contractRepo.GetByContractNumberAsync(request.ContractNumber);

            // Assert
            before.Should().BeEmpty();
            after.Should().HaveCount(1).And.Subject.First().ContractVersion.Should().Be(request.ContractVersion);
        }

        #endregion

        [TestMethod]
        public async Task GetContractRemindersAsync_ReturnsExpectedTest()
        {
            //Arrange
            SetMapperHelper();
            int reminderInterval = 14;
            int pageNumber = 1;
            int pageSize = 1;
            ContractSortOptions sort = ContractSortOptions.LastUpdatedAt;
            SortDirection order = SortDirection.Asc;
            string baseUrl = $"https://localhost:5001";
            string routeTemplateUrl = $"/api/contractReminders?reminderInterval={reminderInterval}&pageNumber={pageNumber}&pageSize={pageSize}&sort={sort}&order={order}";
            const string contractNumber = "Test-Contract-Number";
            const string title = "Test Title";
            DateTime lastEmailReminderSent = DateTime.UtcNow;

            var working = new List<DataModels.Contract>
            {
                new DataModels.Contract { Id = 1, Title = title, ContractNumber = contractNumber, ContractVersion = 1, Ukprn = 12345678, LastEmailReminderSent = lastEmailReminderSent, Status = (int)ContractStatus.PublishedToProvider, FundingType = (int)ContractFundingType.AdvancedLearnerLoans, Year = "2021" },
                new DataModels.Contract { Id = 2, Title = title, ContractNumber = contractNumber, ContractVersion = 2, Ukprn = 12345678, LastEmailReminderSent = lastEmailReminderSent.AddDays(-14), Status = (int)ContractStatus.PublishedToProvider, FundingType = (int)ContractFundingType.AdvancedLearnerLoans, Year = "2021" },
                new DataModels.Contract { Id = 3, Title = title, ContractNumber = contractNumber, ContractVersion = 3, Ukprn = 12345678, LastEmailReminderSent = lastEmailReminderSent.AddDays(-15), Status = (int)ContractStatus.PublishedToProvider, FundingType = (int)ContractFundingType.AdvancedLearnerLoans, Year = "2021" },
            };

            var expectedList = new List<ContractReminderItem>
            {
                new ContractReminderItem { Id = 2, Title = title, ContractNumber = contractNumber, ContractVersion = 2, Ukprn = 12345678, Status = ContractStatus.PublishedToProvider, FundingType = ContractFundingType.AdvancedLearnerLoans }
            };

            var expected = new ContractReminderResponse<IEnumerable<ContractReminderItem>>(expectedList)
            {
                Paging = new Metadata()
                {
                    CurrentPage = pageNumber,
                    HasNextPage = true,
                    HasPreviousPage = false,
                    NextPageUrl = baseUrl + $"/api/contractReminders?reminderInterval={reminderInterval}&pageNumber={pageNumber}&pageSize={pageSize}&sort={sort}&order={order}",
                    PageSize = pageSize,
                    PreviousPageUrl = string.Empty,
                    TotalCount = 2,
                    TotalPages = 2
                }
            };

            ILoggerAdapter<ContractService> logger = new LoggerAdapter<ContractService>(new Logger<ContractService>(new LoggerFactory()));
            ILoggerAdapter<ContractRepository> loggerRepo = new LoggerAdapter<ContractRepository>(new Logger<ContractRepository>(new LoggerFactory()));

            MockAuditService();

            var inMemPdsDbContext = HelperExtensions.GetInMemoryPdsDbContext();
            var repo = new Repository<DataModels.Contract>(inMemPdsDbContext);
            var work = new SingleUnitOfWorkForRepositories(inMemPdsDbContext);
            var contractRepo = new ContractRepository(repo, work, loggerRepo);
            var uriService = new UriService(baseUrl);
            var asposeDocumentManagementContractService = GetDocumentService();
            var contractValidationService = GetContractValidationService();
            var mediator = BuildMediator();
            var contractDocumentService = BuildContractDocumentService();
            var service = new ContractService(contractRepo, _mapper, uriService, logger, _mockAuditService.Object, _semaphoreOnEntity, asposeDocumentManagementContractService, contractValidationService, mediator, contractDocumentService);

            foreach (var item in working)
            {
                await repo.AddAsync(item);
            }

            await work.CommitAsync();

            //Act
            var result = await service.GetContractRemindersAsync(reminderInterval, pageNumber, pageSize, sort, order, routeTemplateUrl);

            //Assert
            result.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public async Task UpdateLastEmailReminderSentAndLastUpdatedAtAsync_ReturnsExpectedResult_Test()
        {
            //Arrange
            SetMapperHelper();
            string baseUrl = $"https://localhost:5001";
            const string contractNumber = "main-000";
            const string title = "Test Title";
            int x = 0;

            var working = new List<DataModels.Contract>
            {
                new DataModels.Contract { Id = 1, Title = title, ContractNumber = string.Empty, ContractVersion = 1, Ukprn = 12345678, LastEmailReminderSent = null, Year = "2021" }
            };

            var request = new UpdateLastEmailReminderSentRequest() { Id = 1, ContractNumber = "main-0001", ContractVersion = 1 };

            foreach (var item in working)
            {
                item.ContractNumber = $"{contractNumber}{x}";
                item.Ukprn += x;
                item.LastEmailReminderSent = null;
                x += 1;
            }

            ILoggerAdapter<ContractService> logger = new LoggerAdapter<ContractService>(new Logger<ContractService>(new LoggerFactory()));
            ILoggerAdapter<ContractRepository> loggerRepo = new LoggerAdapter<ContractRepository>(new Logger<ContractRepository>(new LoggerFactory()));

            MockAuditService();

            var inMemPdsDbContext = HelperExtensions.GetInMemoryPdsDbContext();
            var repo = new Repository<DataModels.Contract>(inMemPdsDbContext);
            var work = new SingleUnitOfWorkForRepositories(inMemPdsDbContext);
            var contractRepo = new ContractRepository(repo, work, loggerRepo);
            var uriService = new UriService(baseUrl);
            var asposeDocumentManagementContractService = GetDocumentService();
            var contractValidationService = GetContractValidationService();
            var mediator = BuildMediator();
            var contractDocumentService = BuildContractDocumentService();
            var service = new ContractService(contractRepo, _mapper, uriService, logger, _mockAuditService.Object, _semaphoreOnEntity, asposeDocumentManagementContractService, contractValidationService, mediator, contractDocumentService);

            foreach (var item in working)
            {
                await repo.AddAsync(item);
            }

            await work.CommitAsync();

            //Act
            var beforeUpdate = await contractRepo.GetAsync(request.Id);

            // assigning to a new variable before this is an in memory db so the
            // LastEmailReminderSent was being populated.
            var actualBeforeUpdate = new DataModels.Contract()
            {
                Id = beforeUpdate.Id,
                Title = beforeUpdate.Title,
                ContractNumber = beforeUpdate.ContractNumber,
                ContractVersion = beforeUpdate.ContractVersion,
                Ukprn = beforeUpdate.Ukprn,
                LastEmailReminderSent = beforeUpdate.LastEmailReminderSent
            };

            var contract = await service.UpdateLastEmailReminderSentAndLastUpdatedAtAsync(request);

            var afterUpdate = await contractRepo.GetAsync(request.Id);

            //Assert
            contract.Should().NotBeNull();
            actualBeforeUpdate.LastEmailReminderSent.Should().BeNull();
            afterUpdate.LastEmailReminderSent.Should().NotBeNull();
            afterUpdate.LastUpdatedAt.Should().BeExactly(afterUpdate.LastEmailReminderSent.Value.TimeOfDay);
        }

        [TestMethod]
        public async Task UpdateContractConfirmApprovalAsync_ReturnsExpectedResult_Test()
        {
            //Arrange
            SetMapperHelper();
            string baseUrl = $"https://localhost:5001";
            const string contractNumber = "Main-0001";
            const string title = "Test Title";

            var working = new DataModels.Contract { Id = 1, Title = title, ContractNumber = contractNumber, ContractVersion = 1, Ukprn = 12345678, Status = (int)ContractStatus.ApprovedWaitingConfirmation, Year = "2021" };

            var request = new UpdateConfirmApprovalRequest() { ContractNumber = "Main-0001", ContractVersion = 1, FileName = BlobHelper.BlobName };

            ILoggerAdapter<ContractService> logger = new LoggerAdapter<ContractService>(new Logger<ContractService>(new LoggerFactory()));
            ILoggerAdapter<ContractRepository> loggerRepo = new LoggerAdapter<ContractRepository>(new Logger<ContractRepository>(new LoggerFactory()));

            MockAuditService();

            var inMemPdsDbContext = HelperExtensions.GetInMemoryPdsDbContext();
            var repo = new Repository<DataModels.Contract>(inMemPdsDbContext);
            var work = new SingleUnitOfWorkForRepositories(inMemPdsDbContext);
            var contractRepo = new ContractRepository(repo, work, loggerRepo);
            var uriService = new UriService(baseUrl);
            var asposeDocumentManagementContractService = GetDocumentService();
            var contractValidationService = GetContractValidationService();
            var mediator = BuildMediator();
            var contractDocumentService = BuildContractDocumentService();
            var service = new ContractService(contractRepo, _mapper, uriService, logger, _mockAuditService.Object, _semaphoreOnEntity, asposeDocumentManagementContractService, contractValidationService, mediator, contractDocumentService);

            await repo.AddAsync(working);

            await work.CommitAsync();

            //Act
            var beforeUpdate = await contractRepo.GetByContractNumberAndVersionWithIncludesAsync(request.ContractNumber, request.ContractVersion, ContractDataEntityInclude.Datas);

            // assigning to a new variable before this is an in memory db so the
            // LastEmailReminderSent was being populated.
            var actualBeforeUpdate = new DataModels.Contract()
            {
                Id = beforeUpdate.Id,
                Title = beforeUpdate.Title,
                ContractNumber = beforeUpdate.ContractNumber,
                ContractVersion = beforeUpdate.ContractVersion,
                Ukprn = beforeUpdate.Ukprn,
                Status = beforeUpdate.Status
            };

            var contract = await service.ConfirmApprovalAsync(request);

            var afterUpdate = await contractRepo.GetByContractNumberAndVersionWithIncludesAsync(request.ContractNumber, request.ContractVersion, ContractDataEntityInclude.Datas);

            //Assert
            contract.Should().NotBeNull();
            actualBeforeUpdate.Status.Should().Be((int)ContractStatus.ApprovedWaitingConfirmation);
            afterUpdate.Status.Should().Be((int)ContractStatus.Approved);
            afterUpdate.ContractData.OriginalContractXml.Should().Be(BlobHelper.BlobSampleContent);
        }

        [TestMethod]
        public async Task UpdateContractWithdrawalAsync_ReturnsExpectedResult_Test()
        {
            //Arrange
            string baseUrl = $"https://localhost:5001";
            SetMapperHelper();
            var contracts = GetDataModel_Contracts();

            var request = new UpdateContractWithdrawalRequest() { ContractNumber = "main-0001", ContractVersion = 1, WithdrawalType = ContractStatus.WithdrawnByAgency, FileName = BlobHelper.BlobName };

            ILoggerAdapter<ContractService> logger = new LoggerAdapter<ContractService>(new Logger<ContractService>(new LoggerFactory()));

            MockAuditService();

            var contractRepo = await GetContractRepository(contracts);
            var uriService = new UriService(baseUrl);
            var asposeDocumentManagementContractService = GetDocumentService();
            var contractValidationService = GetContractValidationService();
            var mediator = BuildMediator();
            var contractDocumentService = BuildContractDocumentService();
            var service = new ContractService(contractRepo, _mapper, uriService, logger, _mockAuditService.Object, _semaphoreOnEntity, asposeDocumentManagementContractService, contractValidationService, mediator, contractDocumentService);

            //Act
            var beforeUpdate = await contractRepo.GetByContractNumberAndVersionWithIncludesAsync(request.ContractNumber, request.ContractVersion, ContractDataEntityInclude.Datas);

            // assigning to a new variable before update because this is an in memory db the
            // update will update the object.
            var actualBeforeUpdate = GetClonedContract(beforeUpdate);

            var contract = await service.WithdrawalAsync(request);

            var afterUpdate = await contractRepo.GetByContractNumberAndVersionWithIncludesAsync(request.ContractNumber, request.ContractVersion, ContractDataEntityInclude.Datas);

            //Assert
            contract.Should().NotBeNull();
            actualBeforeUpdate.Status.Should().Be((int)ContractStatus.PublishedToProvider);
            afterUpdate.Status.Should().Be((int)ContractStatus.WithdrawnByAgency);
            afterUpdate.ContractData.OriginalContractXml.Should().Be(BlobHelper.BlobSampleContent);
        }


        [TestMethod]
        public async Task ApproveManuallyAsync_ReturnsExpectedResult_Test()
        {
            //Arrange
            string baseUrl = $"https://localhost:5001";
            SetMapperHelper();
            var contracts = GetDataModel_ForManualApprove();

            var request = new ContractRequest() { ContractNumber = "main-0001", ContractVersion = 1, FileName = BlobHelper.BlobName };

            ILoggerAdapter<ContractService> logger = new LoggerAdapter<ContractService>(new Logger<ContractService>(new LoggerFactory()));

            MockAuditService();

            var contractRepo = await GetContractRepository(contracts);
            var uriService = new UriService(baseUrl);
            var asposeDocumentManagementContractService = GetDocumentService();
            var contractValidationService = GetContractValidationService();
            var mediator = BuildMediator();
            var contractDocumentService = BuildContractDocumentService();
            var service = new ContractService(contractRepo, _mapper, uriService, logger, _mockAuditService.Object, _semaphoreOnEntity, asposeDocumentManagementContractService, contractValidationService, mediator, contractDocumentService);

            //Act
            var beforeUpdate = await contractRepo.GetByContractNumberAndVersionWithIncludesAsync(request.ContractNumber, request.ContractVersion, ContractDataEntityInclude.Datas | ContractDataEntityInclude.Content);

            // assigning to a new variable before update because this is an in memory db the
            // update will update the object.
            var actualBeforeUpdate = GetClonedContract(beforeUpdate);

            var contract = await service.ApproveManuallyAsync(request);

            var afterUpdate = await contractRepo.GetByContractNumberAndVersionWithIncludesAsync(request.ContractNumber, request.ContractVersion, ContractDataEntityInclude.Datas | ContractDataEntityInclude.Content);

            //Assert
            contract.Should().NotBeNull();
            actualBeforeUpdate.Status.Should().Be((int)ContractStatus.PublishedToProvider);
            afterUpdate.Status.Should().Be((int)ContractStatus.Approved);
            afterUpdate.ContractData.OriginalContractXml.Should().Be(BlobHelper.BlobSampleContent);
        }

        [TestMethod]
        public async Task ApproveManuallyAsync_ReturnsExpectedResult_DocumentTest()
        {
            //Arrange
            SetAsposeLicense();
            var signer = $"hand and approved by DfE";
            string baseUrl = $"https://localhost:5001";
            SetMapperHelper();
            var contracts = GetDataModel_ForManualApprove();

            var request = new ContractRequest() { ContractNumber = "main-0001", ContractVersion = 1, FileName = BlobHelper.BlobName };

            ILoggerAdapter<ContractService> logger = new LoggerAdapter<ContractService>(new Logger<ContractService>(new LoggerFactory()));

            MockAuditService();

            var contractRepo = await GetContractRepository(contracts);
            var uriService = new UriService(baseUrl);
            var asposeDocumentManagementContractService = GetDocumentService();
            var contractValidationService = GetContractValidationService();
            var mediator = BuildMediator();
            var contractDocumentService = BuildContractDocumentService();
            var service = new ContractService(contractRepo, _mapper, uriService, logger, _mockAuditService.Object, _semaphoreOnEntity, asposeDocumentManagementContractService, contractValidationService, mediator, contractDocumentService);

            //Act
            var beforeUpdate = await contractRepo.GetByContractNumberAndVersionWithIncludesAsync(request.ContractNumber, request.ContractVersion, ContractDataEntityInclude.Datas | ContractDataEntityInclude.Content);

            // assigning to a new variable before update because this is an in memory db the
            // update will update the object.
            var actualBeforeUpdate = GetClonedContract(beforeUpdate);

            var contract = await service.ApproveManuallyAsync(request);

            var afterUpdate = await contractRepo.GetByContractNumberAndVersionWithIncludesAsync(request.ContractNumber, request.ContractVersion, ContractDataEntityInclude.Datas);



            //Assert
            contract.Should().NotBeNull();
            actualBeforeUpdate.Status.Should().Be((int)ContractStatus.PublishedToProvider);
            afterUpdate.Status.Should().Be((int)ContractStatus.Approved);
            afterUpdate.ContractContent.Content.ShouldHaveSignedPage("testDoc", signer, afterUpdate.SignedOn.Value, true, afterUpdate.ContractContent.FileName, ContractFundingType.CityDeals, null);
            afterUpdate.ContractData.OriginalContractXml.Should().Be(BlobHelper.BlobSampleContent);
        }

        [TestMethod]
        public async Task PrependSignedPageToDocumentAndSaveAsync_AfterSuccesfullyPrependSignedPage_SaveTheContractContentToDatabase()
        {
            //Arrange
            SetAsposeLicense();
            string baseUrl = $"https://localhost:5001";
            var contracts = GetDataModel_Contracts(true);

            ILoggerAdapter<ContractService> logger = new LoggerAdapter<ContractService>(new Logger<ContractService>(new LoggerFactory()));
            MockAuditService();
            var contractRepo = await GetContractRepository(contracts);
            var uriService = new UriService(baseUrl);
            var asposeDocumentManagementContractService = GetDocumentService();
            var contractValidationService = GetContractValidationService();
            var mediator = BuildMediator();
            var contractDocumentService = BuildContractDocumentService();
            var service = new ContractService(contractRepo, _mapper, uriService, logger, _mockAuditService.Object, _semaphoreOnEntity, asposeDocumentManagementContractService, contractValidationService, mediator, contractDocumentService);

            //Act
            await service.PrependSignedPageToDocumentAndSaveAsync(1);
            var afterUpdate = await contractRepo.GetAsync(1);

            //Assert
            afterUpdate.ContractContent.Content.ShouldHaveSignedPage("testDoc", afterUpdate.SignedByDisplayName, afterUpdate.SignedOn.Value, false, afterUpdate.ContractContent.FileName, ContractFundingType.CityDeals, null);
        }

        private void SetAsposeLicense()
        {
            var mockServiceCollection = new Mock<IServiceCollection>();
            AsposeLicenceManagement.AddAsposeLicense(mockServiceCollection.Object);
        }


        private DataModels.Contract GetClonedContract(DataModels.Contract contract)
        {
            return new DataModels.Contract()
            {
                Id = contract.Id,
                Title = contract.Title,
                ContractNumber = contract.ContractNumber,
                ContractVersion = contract.ContractVersion,
                Ukprn = contract.Ukprn,
                Status = contract.Status
            };
        }

        private List<DataModels.Contract> GetDataModel_Contracts(bool isApproved = false)
        {
            const string contractNumber = "main-000";
            const string title = "Test Title";
            int x = 1;

            var contracts = new List<DataModels.Contract>();

            if (isApproved)
            {
                contracts.Add(new DataModels.Contract
                {
                    Id = 1,
                    Title = title,
                    ContractNumber = contractNumber,
                    ContractVersion = 1,
                    Ukprn = 12345678,
                    Status = (int)ContractStatus.Approved,
                    FundingType = (int)ContractFundingType.CityDeals,
                    Year = "2021",
                    SignedByDisplayName = "testsuser",
                    SignedOn = DateTime.Now,
                    ContractContent = GetDummyContractContent(1)
                });
            }
            else
            {
                contracts.Add(new DataModels.Contract
                {
                    Id = 1,
                    Title = title,
                    ContractNumber = contractNumber,
                    ContractVersion = 1,
                    Ukprn = 12345678,
                    Status = (int)ContractStatus.PublishedToProvider,
                    FundingType = (int)ContractFundingType.CityDeals,
                    Year = "2021",
                    ContractContent = GetDummyContractContent(1)
                });
            }

            foreach (var item in contracts)
            {
                item.ContractNumber = $"{contractNumber}{x}";
                item.Ukprn += x;
                x += 1;
            }

            return contracts;
        }

        private List<DataModels.Contract> GetDataModel_ForManualApprove()
        {
            const string contractNumber = "main-000";
            const string title = "Test Title";
            int x = 1;

            var contracts = new List<DataModels.Contract>
            {
                new DataModels.Contract { Id = 1, Title = title, ContractNumber = string.Empty, ContractVersion = 1, Ukprn = 12345678, Status = (int)ContractStatus.PublishedToProvider, FundingType = (int)ContractFundingType.CityDeals, ContractContent = GetDummyContractContent(1), Year = "2021" }
            };

            foreach (var item in contracts)
            {
                item.ContractNumber = $"{contractNumber}{x}";
                item.Ukprn += x;
                x += 1;
            }

            return contracts;
        }

        private DataModels.ContractContent GetDummyContractContent(int id)
        {
            var pdfContent = GetPdfContent();
            return new DataModels.ContractContent()
            {
                Id = id,
                FileName = "testDoc.pdf",
                Content = pdfContent,
                Size = pdfContent.Length
            };
        }

        private byte[] GetPdfContent()
        {
            var pdfResources = "Pds.Contracts.Data.Services.Tests.Integration.Resources.test.pdf";
            using (Stream resFilestream = typeof(ContractServiceTests).Assembly.GetManifestResourceStream(pdfResources))
            {
                byte[] ba = new byte[resFilestream.Length];
                resFilestream.Read(ba, 0, ba.Length);
                return ba;
            }
        }

        private async Task<ContractRepository> GetContractRepository(List<DataModels.Contract> contracts)
        {
            ILoggerAdapter<ContractRepository> loggerRepo = new LoggerAdapter<ContractRepository>(new Logger<ContractRepository>(new LoggerFactory()));
            var inMemPdsDbContext = HelperExtensions.GetInMemoryPdsDbContext();
            var repo = new Repository<DataModels.Contract>(inMemPdsDbContext);
            var work = new SingleUnitOfWorkForRepositories(inMemPdsDbContext);
            var contractRepo = new ContractRepository(repo, work, loggerRepo);

            foreach (var item in contracts)
            {
                await repo.AddAsync(item);
            }

            await work.CommitAsync();

            return contractRepo;
        }


        /// <summary>
        /// Set the mapper config.
        /// </summary>
        private void SetMapperHelper()
        {
            var mapperConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new ContractMapperProfile());
            });

            _mapper = mapperConfig.CreateMapper();
        }

        private void MockAuditService()
        {
            _mockAuditService = new Mock<IAuditService>();

            _mockAuditService
                .Setup(e => e.AuditAsync(It.IsAny<AuditModels.Audit>()))
                .Returns(Task.CompletedTask)
                .Verifiable();
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
                Year = "2021 to 2030",
                Type = ContractType.ConditionsOfFundingGrant,
                ParentContractNumber = null,
                StartDate = startDate,
                EndDate = endDate,
                AmendmentType = ContractAmendmentType.None,
                ContractAllocationNumber = null,
                CreatedBy = "Feed",
                ContractContent = new CreateContractRequestDocument()
                {
                    Content = new byte[20],
                    Size = 20,
                    FileName = "Test file"
                },
                PageCount = 0,
                ContractData = BlobHelper.BlobName,
                ContractFundingStreamPeriodCodes = new CreateContractCode[] { new CreateContractCode() { Code = "Test" } }
            };
            return request;
        }

        private AsposeDocumentManagementContractService GetDocumentService()
        {
            ILoggerAdapter<AsposeDocumentManagementService> logger = new LoggerAdapter<AsposeDocumentManagementService>(new Logger<AsposeDocumentManagementService>(new LoggerFactory()));
            return new AsposeDocumentManagementContractService(logger);
        }

        private ContractValidationService GetContractValidationService()
        {
            return new ContractValidationService();
        }

        private IMediator BuildMediator()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.development.json", optional: false, reloadOnChange: true)
                .Build();

            var services = new ServiceCollection();
            services.AddLoggerAdapter();
            services.AddAutoMapper(typeof(FeatureServiceCollectionExtensions).Assembly);
            var policyRegistry = services.AddPolicyRegistry();
            services.AddAuditApiClient(configuration, policyRegistry);
            services.AddSingleton<ITopicClient>(serviceProvider => ServiceBusHelper.GetTopicClient(configuration));
            services.AddSingleton<IMessagePublisher, MessagePublisher>();

            services.AddMediatR(typeof(FeatureServiceCollectionExtensions).Assembly);

            var provider = services.BuildServiceProvider();

            return provider.GetRequiredService<IMediator>();
        }

        private IContractDocumentService BuildContractDocumentService()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.development.json", optional: false, reloadOnChange: true)
                .Build();

            var services = new ServiceCollection();
            services.AddLoggerAdapter();
            services.AddAutoMapper(typeof(FeatureServiceCollectionExtensions).Assembly);
            var policyRegistry = services.AddPolicyRegistry();
            services.AddAuditApiClient(configuration, policyRegistry);

            services.AddSingleton<IContractDocumentService>(provider =>
            new ContractDocumentService(
                Helpers.BlobHelper.GetBlobContainerClient(configuration),
                provider.GetRequiredService<ILoggerAdapter<IContractDocumentService>>()));

            var provider = services.BuildServiceProvider();

            return provider.GetRequiredService<IContractDocumentService>();
        }
    }
}