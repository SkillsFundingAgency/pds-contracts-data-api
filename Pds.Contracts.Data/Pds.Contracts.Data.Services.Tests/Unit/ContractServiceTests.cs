using AutoMapper;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Audit.Api.Client.Interfaces;
using Pds.Contracts.Data.Common.CustomExceptionHandlers;
using Pds.Contracts.Data.Common.Enums;
using Pds.Contracts.Data.Common.Responses;
using Pds.Contracts.Data.Repository.Implementations;
using Pds.Contracts.Data.Repository.Interfaces;
using Pds.Contracts.Data.Services.Implementations;
using Pds.Contracts.Data.Services.Interfaces;
using Pds.Contracts.Data.Services.Models;
using Pds.Contracts.Data.Services.Responses;
using Pds.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuditModels = Pds.Audit.Api.Client.Models;
using DataModels = Pds.Contracts.Data.Repository.DataModels;

namespace Pds.Contracts.Data.Services.Tests.Unit
{
    [TestClass, TestCategory("Unit")]
    public class ContractServiceTests
    {
        private readonly IContractRepository _contractRepository
            = Mock.Of<IContractRepository>(MockBehavior.Strict);

        private readonly IMapper _mapper
            = Mock.Of<IMapper>(MockBehavior.Strict);

        private readonly Mock<IAuditService> _mockAuditService
            = new Mock<IAuditService>(MockBehavior.Strict);

        private readonly Mock<ILoggerAdapter<ContractService>> _mockLogger
            = new Mock<ILoggerAdapter<ContractService>>(MockBehavior.Strict);

        private readonly ISemaphoreOnEntity<string> _semaphoreOnEntity
            = Mock.Of<ISemaphoreOnEntity<string>>();

        private IUriService _mockUriService = Mock.Of<IUriService>(MockBehavior.Strict);

        #region CreateAsync

        [TestMethod]
        public async Task Create_WhenContractDoesNotAlreadyExist_Then_ContractIsAddedToDatabase()
        {
            // Arrange
            CreateContractRequest createRequest = Generate_CreateContractRequest();

            SetupAuditService_TrySendAuditAsyncMethod();
            SetupLogger_LogInformationMethod();

            IEnumerable<DataModels.Contract> matchedRecords = new List<DataModels.Contract>();

            var contractRecord = new DataModels.Contract();

            Mock.Get(_mapper)
                .Setup(p => p.Map<DataModels.Contract>(createRequest))
                .Returns(contractRecord)
                .Verifiable();

            SetupRepository_GetByContractNumberAsyncMethod(createRequest, matchedRecords);
            SetupRepository_CreateAsyncMethod(contractRecord, Task.CompletedTask);

            var service = GetContractService();

            // Act
            await service.CreateAsync(createRequest);

            // Assert
            VerifyAll();
        }

        [TestMethod]
        public async Task Create_WhenContractAlreadyExists_WithALowerVersion_Then_ContractIsAddedToDatabase()
        {
            // Arrange
            CreateContractRequest createRequest = Generate_CreateContractRequest();

            SetupAuditService_TrySendAuditAsyncMethod();
            SetupLogger_LogInformationMethod();

            IEnumerable<DataModels.Contract> matchedRecords = new List<DataModels.Contract>()
            {
                new DataModels.Contract()
                {
                    ContractNumber = createRequest.ContractNumber,
                    ContractVersion = createRequest.ContractVersion - 1
                }
            };

            var contractRecord = new DataModels.Contract();

            Mock.Get(_mapper)
                .Setup(p => p.Map<DataModels.Contract>(createRequest))
                .Returns(contractRecord)
                .Verifiable();

            SetupRepository_GetByContractNumberAsyncMethod(createRequest, matchedRecords);
            SetupRepository_CreateAsyncMethod(contractRecord, Task.CompletedTask);

            var service = GetContractService();

            // Act
            await service.CreateAsync(createRequest);

            // Assert
            VerifyAll();
        }

        [TestMethod]
        public void Create_WhenContractAlreadyExists_WithTheSameContractVersion_Then_DuplicateContractExceptionIsRaised()
        {
            // Arrange
            CreateContractRequest createRequest = Generate_CreateContractRequest();

            SetupLogger_LogInformationMethod();

            IEnumerable<DataModels.Contract> matchedRecords = new List<DataModels.Contract>()
            {
                new DataModels.Contract()
                {
                    ContractNumber = createRequest.ContractNumber,
                    ContractVersion = createRequest.ContractVersion
                }
            };

            SetupRepository_GetByContractNumberAsyncMethod(createRequest, matchedRecords);

            var service = GetContractService();

            // Act
            Func<Task> act = async () => await service.CreateAsync(createRequest);

            // Assert
            act.Should().Throw<DuplicateContractException>();

            VerifyAll();
        }

        [TestMethod]
        public void Create_WhenContractAlreadyExists_WithAHigherContractVersion_Then_InvalidContractVersionExceptionIsRaised()
        {
            // Arrange
            CreateContractRequest createRequest = Generate_CreateContractRequest();

            SetupLogger_LogInformationMethod();

            IEnumerable<DataModels.Contract> matchedRecords = new List<DataModels.Contract>()
            {
                new DataModels.Contract()
                {
                    ContractNumber = createRequest.ContractNumber,
                    ContractVersion = createRequest.ContractVersion + 1
                }
            };

            SetupRepository_GetByContractNumberAsyncMethod(createRequest, matchedRecords);

            var service = GetContractService();

            // Act
            Func<Task> act = async () => await service.CreateAsync(createRequest);

            // Assert
            act.Should().Throw<ContractWithHigherVersionAlreadyExistsException>();

            VerifyAll();
        }

        [TestMethod]
        public async Task Create_IfCreateAuditFails_Then_NoExceptionIsRaised()
        {
            // Arrange
            CreateContractRequest createRequest = Generate_CreateContractRequest();

            SetupAuditService_TrySendAuditAsyncMethod();
            SetupLogger_LogInformationMethod();

            IEnumerable<DataModels.Contract> matchedRecords = new List<DataModels.Contract>();

            var contractRecord = new DataModels.Contract();

            Mock.Get(_mapper)
                .Setup(p => p.Map<DataModels.Contract>(createRequest))
                .Returns(contractRecord)
                .Verifiable();

            SetupRepository_GetByContractNumberAsyncMethod(createRequest, matchedRecords);
            SetupRepository_CreateAsyncMethod(contractRecord, Task.CompletedTask);

            var service = GetContractService();

            // Act
            await service.CreateAsync(createRequest);

            // Assert
            VerifyAll();
        }

        #endregion

        [TestMethod]
        public async Task GetAsync_ReturnsExpectedResult()
        {
            // Arrange
            var expectedServiceModel = Mock.Of<Models.Contract>();
            var expectedDataModel = Mock.Of<DataModels.Contract>();
            var mockRepo = Mock.Of<IContractRepository>(MockBehavior.Strict);
            Mock.Get(mockRepo)
                .Setup(r => r.GetAsync(It.IsAny<int>()))
                .ReturnsAsync(expectedDataModel)
                .Verifiable();

            SetUpMockUriService("action");

            SetupLogger_LogInformationMethod();

            var mockMapper = Mock.Of<IMapper>();
            Mock.Get(mockMapper)
                .Setup(m => m.Map<Models.Contract>(It.IsAny<DataModels.Contract>()))
                .Returns(expectedServiceModel)
                .Verifiable();

            SetupAuditService_AuditAsyncMethod();

            var contractService = new ContractService(mockRepo, mockMapper, _mockUriService, _mockLogger.Object, _mockAuditService.Object, _semaphoreOnEntity);

            // Act
            var actual = await contractService.GetAsync(1);

            // Assert
            actual.Should().Be(expectedServiceModel);
            Mock.Get(mockRepo).Verify(r => r.GetAsync(It.IsAny<int>()), Times.Once);
            Mock.Get(mockMapper).Verify(m => m.Map<Models.Contract>(It.IsAny<DataModels.Contract>()), Times.Once);
        }

        [TestMethod]
        public async Task GetByContractNumber_ReturnsExpectedResultAsync()
        {
            // Arrange
            var expectedServiceModelCollection = Mock.Of<IList<Models.Contract>>();
            var expectedDataModelCollection = Mock.Of<IQueryable<DataModels.Contract>>();
            var mockRepo = Mock.Of<IContractRepository>(MockBehavior.Strict);
            Mock.Get(mockRepo)
                .Setup(r => r.GetByContractNumberAsync(It.IsAny<string>()))
                .ReturnsAsync(expectedDataModelCollection)
                .Verifiable();

            SetUpMockUriService("action");

            SetupLogger_LogInformationMethod();

            var mockMapper = Mock.Of<IMapper>();
            Mock.Get(mockMapper)
                .Setup(m => m.Map<IList<Models.Contract>>(It.IsAny<IQueryable<DataModels.Contract>>()))
                .Returns(expectedServiceModelCollection)
                .Verifiable();

            SetupAuditService_AuditAsyncMethod();

            var contractService = new ContractService(mockRepo, mockMapper, _mockUriService, _mockLogger.Object, _mockAuditService.Object, _semaphoreOnEntity);

            // Act
            var actual = await contractService.GetByContractNumberAsync("some-contract-number");

            // Assert
            actual.Should().BeEquivalentTo(expectedServiceModelCollection);
            Mock.Get(mockRepo).Verify(r => r.GetByContractNumberAsync("some-contract-number"), Times.Once);
            Mock.Get(mockMapper).Verify(m => m.Map<IList<Models.Contract>>(It.IsAny<IQueryable<DataModels.Contract>>()), Times.Once);
        }

        [TestMethod]
        public async Task GetByContractNumberAndVersion_ReturnsExpectedResultAsync()
        {
            // Arrange
            var expectedServiceModel = Mock.Of<Models.Contract>();
            var expectedDataModel = Mock.Of<DataModels.Contract>();
            var mockRepo = Mock.Of<IContractRepository>(MockBehavior.Strict);
            Mock.Get(mockRepo)
                .Setup(r => r.GetByContractNumberAndVersionAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(expectedDataModel)
                .Verifiable();

            SetUpMockUriService("action");

            SetupLogger_LogInformationMethod();

            var mockMapper = Mock.Of<IMapper>();
            Mock.Get(mockMapper)
                .Setup(m => m.Map<Models.Contract>(It.IsAny<DataModels.Contract>()))
                .Returns(expectedServiceModel)
                .Verifiable();

            SetupAuditService_AuditAsyncMethod();

            var contractService = new ContractService(mockRepo, mockMapper, _mockUriService, _mockLogger.Object, _mockAuditService.Object, _semaphoreOnEntity);

            // Act
            var actual = await contractService.GetByContractNumberAndVersionAsync("some-contract-number", 1);

            // Assert
            actual.Should().Be(expectedServiceModel);
            Mock.Get(mockRepo).Verify(r => r.GetByContractNumberAndVersionAsync("some-contract-number", 1), Times.Once);
            Mock.Get(mockMapper).Verify(m => m.Map<Models.Contract>(It.IsAny<DataModels.Contract>()), Times.Once);
        }

        [TestMethod]
        public async Task GetContractRemindersAsync_ReturnsExpectedResultAsync()
        {
            // Arrange
            int reminderInterval = 14;
            int pageNumber = 1;
            int pageSize = 1;
            ContractSortOptions sort = ContractSortOptions.LastUpdatedAt;
            SortDirection order = SortDirection.Asc;
            DateTime currentDateTimeMinusNumberOfDays = DateTime.UtcNow.Date.AddDays(-reminderInterval).AddHours(23).AddMinutes(59);
            string requestPath = "/api/contractReminder";
            string queryString = "?reminderInterval=14&page={page}&count=2";
            string routeTemplateURL = requestPath + queryString;

            var expectedDummyMetadata = new Metadata() { CurrentPage = 1, PageSize = pageSize, TotalCount = 1, TotalPages = 1, NextPageUrl = string.Empty, PreviousPageUrl = string.Empty };

            var expectedDummyReminderItems = new List<ContractReminderItem>();
            var expectedDummyServiceModel = new ContractReminderResponse<IEnumerable<ContractReminderItem>>(expectedDummyReminderItems)
            {
                Paging = expectedDummyMetadata
            };

            var expectedDummyDataModel = new PagedList<DataModels.Contract>(new List<DataModels.Contract>(), 1, 1, 1);

            var mockRepo = Mock.Of<IContractRepository>(MockBehavior.Strict);
            Mock.Get(mockRepo)
                .Setup(r => r.GetContractRemindersAsync(It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<ContractSortOptions>(), It.IsAny<SortDirection>()))
               .ReturnsAsync(expectedDummyDataModel)
               .Verifiable();

            SetUpMockUriService(routeTemplateURL);
            SetupLogger_LogInformationMethod();

            var mockMapper = Mock.Of<IMapper>();
            Mock.Get(mockMapper)
                .Setup(m => m.Map<IEnumerable<Models.ContractReminderItem>>(new List<DataModels.Contract>()))
                .Returns(expectedDummyReminderItems)
                .Verifiable();

            SetupAuditService_AuditAsyncMethod();

            var contractService = new ContractService(mockRepo, mockMapper, _mockUriService, _mockLogger.Object, _mockAuditService.Object, _semaphoreOnEntity);

            // Act
            var actual = await contractService.GetContractRemindersAsync(reminderInterval, pageNumber, pageSize, sort, order, routeTemplateURL);

            // Assert
            actual.Should().BeEquivalentTo(expectedDummyServiceModel);
            Mock.Get(mockRepo).Verify(r => r.GetContractRemindersAsync(currentDateTimeMinusNumberOfDays, pageNumber, pageSize, sort, order), Times.Once);
            Mock.Get(mockMapper).Verify(m => m.Map<IEnumerable<Models.ContractReminderItem>>(new List<DataModels.Contract>()), Times.Once);
            _mockLogger.Verify();
        }

        [TestMethod]
        public async Task UpdateLastEmailReminderSentAndLastUpdatedAt_TestAsync_SuccessResultExpected()
        {
            // Arrange
            Contract dummyServiceModel = new Contract() { Id = 1, ContractVersion = 1, ContractNumber = "abc" };
            var expectedDataModel = Mock.Of<DataModels.Contract>();
            var mockRepo = Mock.Of<IContractRepository>(MockBehavior.Strict);
            Mock.Get(mockRepo)
              .Setup(r => r.UpdateLastEmailReminderSentAndLastUpdatedAtAsync(It.IsAny<int>()))
              .ReturnsAsync(expectedDataModel)
              .Verifiable();

            string actionUrl = "action";
            var mockUriService = Mock.Of<IUriService>(MockBehavior.Strict);
            Mock.Get(mockUriService)
                .Setup(m => m.GetUri(actionUrl))
                .Returns(It.IsAny<Uri>())
                .Verifiable();

            var mockMapper = Mock.Of<IMapper>();
            Mock.Get(mockMapper)
                .Setup(m => m.Map<Models.Contract>(It.IsAny<DataModels.Contract>()))
                .Returns(dummyServiceModel)
                .Verifiable();

            SetupAuditService_AuditAsyncMethod();

            var contractService = new ContractService(mockRepo, mockMapper, mockUriService, _mockLogger.Object, _mockAuditService.Object, _semaphoreOnEntity);

            var reminder = GetASingleUpdateLastEmailReminderSentRequest();

            // Act
            var result = await contractService.UpdateLastEmailReminderSentAndLastUpdatedAtAsync(reminder);

            // Assert
            result.Should().NotBeNull();
            Mock.Get(mockRepo).Verify(r => r.UpdateLastEmailReminderSentAndLastUpdatedAtAsync(reminder.Id), Times.Exactly(1));
            Mock.Get(mockMapper).Verify(m => m.Map<Models.Contract>(It.IsAny<DataModels.Contract>()), Times.Once);
            _mockLogger.Verify();
        }

        [TestMethod]
        public async Task UpdateContractConfirmApprovalAsync_TestAsync_SuccessResultExpected()
        {
            // Arrange
            var mockDataModel = Mock.Of<UpdatedContractStatusResponse>();
            var mockRepo = Mock.Of<IContractRepository>(MockBehavior.Strict);
            Mock.Get(mockRepo)
              .Setup(r => r.UpdateContractStatusAsync(It.IsAny<int>(), It.IsAny<ContractStatus>(), It.IsAny<ContractStatus>()))
              .ReturnsAsync(mockDataModel)
              .Verifiable();

            string actionUrl = "action";
            var mockUriService = Mock.Of<IUriService>(MockBehavior.Strict);
            Mock.Get(mockUriService)
                .Setup(m => m.GetUri(actionUrl))
                .Returns(It.IsAny<Uri>())
                .Verifiable();

            SetupLogger_LogInformationMethod();

            var mockMapper = Mock.Of<IMapper>();

            SetupAuditService_AuditAsyncMethod();

            var contractService = new ContractService(mockRepo, mockMapper, mockUriService, _mockLogger.Object, _mockAuditService.Object, _semaphoreOnEntity);

            var reminder = GetASingleUpdateConfirmApprovalRequest();

            // Act
            var result = await contractService.UpdateContractConfirmApprovalAsync(reminder);

            // Assert
            result.Should().NotBeNull();
            Mock.Get(mockRepo).Verify(r => r.UpdateContractStatusAsync(It.IsAny<int>(), It.IsAny<ContractStatus>(), It.IsAny<ContractStatus>()), Times.Exactly(1));
            _mockAuditService.Verify(e => e.AuditAsync(It.IsAny<AuditModels.Audit>()), Times.Once);
            _mockLogger.Verify();
        }

        [TestMethod]
        public void UpdateContractConfirmApprovalAsync_TestAsync_ResultContractStatusExceptionExpected()
        {
            // Arrange
            var mockDataModel = Mock.Of<UpdatedContractStatusResponse>();
            var mockRepo = Mock.Of<IContractRepository>(MockBehavior.Strict);
            Mock.Get(mockRepo)
              .Setup(r => r.UpdateContractStatusAsync(It.IsAny<int>(), It.IsAny<ContractStatus>(), It.IsAny<ContractStatus>()))
              .Throws(new ContractStatusException("Contract status is not ApprovedWaitingConfirmation."))
              .Verifiable();

            string actionUrl = "action";
            var mockUriService = Mock.Of<IUriService>(MockBehavior.Strict);
            Mock.Get(mockUriService)
                .Setup(m => m.GetUri(actionUrl))
                .Returns(It.IsAny<Uri>())
                .Verifiable();

            SetupLogger_LogInformationMethod();

            var mockMapper = Mock.Of<IMapper>();

            SetupAuditService_AuditAsyncMethod();

            var contractService = new ContractService(mockRepo, mockMapper, mockUriService, _mockLogger.Object, _mockAuditService.Object, _semaphoreOnEntity);

            var reminder = GetASingleUpdateConfirmApprovalRequest();

            // Act
            Func<Task> act = async () => await contractService.UpdateContractConfirmApprovalAsync(reminder);

            // Assert
            act.Should().Throw<ContractStatusException>();
            Mock.Get(mockRepo).Verify(r => r.UpdateContractStatusAsync(It.IsAny<int>(), It.IsAny<ContractStatus>(), It.IsAny<ContractStatus>()), Times.Once);
            _mockAuditService.Verify(e => e.AuditAsync(It.IsAny<AuditModels.Audit>()), Times.Never);
            _mockLogger.Verify();
        }

        [TestMethod]
        public void UpdateContractConfirmApprovalAsync_TestAsync_ResultContractNotFoundExceptionExpected()
        {
            // Arrange
            var mockDataModel = Mock.Of<UpdatedContractStatusResponse>();
            var mockRepo = Mock.Of<IContractRepository>(MockBehavior.Strict);
            Mock.Get(mockRepo)
              .Setup(r => r.UpdateContractStatusAsync(It.IsAny<int>(), It.IsAny<ContractStatus>(), It.IsAny<ContractStatus>()))
              .Throws(new ContractNotFoundException("Contract was not found."))
              .Verifiable();

            string actionUrl = "action";
            var mockUriService = Mock.Of<IUriService>(MockBehavior.Strict);
            Mock.Get(mockUriService)
                .Setup(m => m.GetUri(actionUrl))
                .Returns(It.IsAny<Uri>())
                .Verifiable();

            SetupLogger_LogInformationMethod();

            var mockMapper = Mock.Of<IMapper>();

            SetupAuditService_AuditAsyncMethod();

            var contractService = new ContractService(mockRepo, mockMapper, mockUriService, _mockLogger.Object, _mockAuditService.Object, _semaphoreOnEntity);

            var reminder = GetASingleUpdateConfirmApprovalRequest();

            // Act
            Func<Task> act = async () => await contractService.UpdateContractConfirmApprovalAsync(reminder);

            // Assert
            act.Should().Throw<ContractNotFoundException>();
            Mock.Get(mockRepo).Verify(r => r.UpdateContractStatusAsync(It.IsAny<int>(), It.IsAny<ContractStatus>(), It.IsAny<ContractStatus>()), Times.Once);
            _mockAuditService.Verify(e => e.AuditAsync(It.IsAny<AuditModels.Audit>()), Times.Never);
            _mockLogger.Verify();
        }

        [TestMethod]
        public void UpdateContractConfirmApprovalAsync_TestAsync_ResultGenericExceptionExpected()
        {
            // Arrange
            var mockDataModel = Mock.Of<UpdatedContractStatusResponse>();
            var mockRepo = Mock.Of<IContractRepository>(MockBehavior.Strict);
            Mock.Get(mockRepo)
              .Setup(r => r.UpdateContractStatusAsync(It.IsAny<int>(), It.IsAny<ContractStatus>(), It.IsAny<ContractStatus>()))
              .Throws(new Exception())
              .Verifiable();

            string actionUrl = "action";
            var mockUriService = Mock.Of<IUriService>(MockBehavior.Strict);
            Mock.Get(mockUriService)
                .Setup(m => m.GetUri(actionUrl))
                .Returns(It.IsAny<Uri>())
                .Verifiable();

            SetupLogger_LogInformationMethod();

            var mockMapper = Mock.Of<IMapper>();

            SetupAuditService_AuditAsyncMethod();

            var contractService = new ContractService(mockRepo, mockMapper, mockUriService, _mockLogger.Object, _mockAuditService.Object, _semaphoreOnEntity);

            var reminder = GetASingleUpdateConfirmApprovalRequest();

            // Act
            Func<Task> act = async () => await contractService.UpdateContractConfirmApprovalAsync(reminder);

            // Assert
            act.Should().Throw<Exception>();
            Mock.Get(mockRepo).Verify(r => r.UpdateContractStatusAsync(It.IsAny<int>(), It.IsAny<ContractStatus>(), It.IsAny<ContractStatus>()), Times.Once);
            _mockAuditService.Verify(e => e.AuditAsync(It.IsAny<AuditModels.Audit>()), Times.Never);
            _mockLogger.Verify();
        }

        [TestMethod]
        public async Task UpdateContractConfirmApprovalAsync_TestAsync_SuccessResultAuditErrorSlientExpected()
        {
            // Arrange
            var mockDataModel = Mock.Of<UpdatedContractStatusResponse>();
            var mockRepo = Mock.Of<IContractRepository>(MockBehavior.Strict);
            Mock.Get(mockRepo)
              .Setup(r => r.UpdateContractStatusAsync(It.IsAny<int>(), It.IsAny<ContractStatus>(), It.IsAny<ContractStatus>()))
              .ReturnsAsync(mockDataModel)
              .Verifiable();

            string actionUrl = "action";
            var mockUriService = Mock.Of<IUriService>(MockBehavior.Strict);
            Mock.Get(mockUriService)
                .Setup(m => m.GetUri(actionUrl))
                .Returns(It.IsAny<Uri>())
                .Verifiable();

            SetupLogger_LogInformationMethod();

            _mockLogger
                .Setup(logger => logger.LogError(It.IsAny<string>()))
                .Verifiable();

            var mockMapper = Mock.Of<IMapper>();

            SetupAuditService_AuditAsyncThrowsExceptionMethod();

            var contractService = new ContractService(mockRepo, mockMapper, mockUriService, _mockLogger.Object, _mockAuditService.Object, _semaphoreOnEntity);

            var reminder = GetASingleUpdateConfirmApprovalRequest();

            // Act
            var result = await contractService.UpdateContractConfirmApprovalAsync(reminder);

            // Assert
            result.Should().NotBeNull();
            Mock.Get(mockRepo).Verify(r => r.UpdateContractStatusAsync(It.IsAny<int>(), It.IsAny<ContractStatus>(), It.IsAny<ContractStatus>()), Times.Exactly(1));
            _mockAuditService.Verify(e => e.AuditAsync(It.IsAny<AuditModels.Audit>()), Times.Once);
            _mockLogger.Verify();
        }

        #region Arrange Helpers

        [TestMethod]
        public async Task UpdateContractWithdrawalAsync_TestAsync_SuccessResultExpected()
        {
            // Arrange
            var mockDataModel = Mock.Of<UpdatedContractStatusResponse>();
            var mockRepo = Mock.Of<IContractRepository>(MockBehavior.Strict);
            Mock.Get(mockRepo)
              .Setup(r => r.UpdateContractStatusAsync(It.IsAny<int>(), It.IsAny<ContractStatus>(), It.IsAny<ContractStatus>()))
              .ReturnsAsync(mockDataModel)
              .Verifiable();

            string actionUrl = "action";
            var mockUriService = Mock.Of<IUriService>(MockBehavior.Strict);
            Mock.Get(mockUriService)
                .Setup(m => m.GetUri(actionUrl))
                .Returns(It.IsAny<Uri>())
                .Verifiable();

            SetupLogger_LogInformationMethod();

            var mockMapper = Mock.Of<IMapper>();

            SetupAuditService_TrySendAuditAsyncMethod();

            var contractService = new ContractService(mockRepo, mockMapper, mockUriService, _mockLogger.Object, _mockAuditService.Object, _semaphoreOnEntity);

            var request = GetASingleUpdateContractWithdrawalRequest();

            // Act
            var result = await contractService.UpdateContractWithdrawalAsync(request);

            // Assert
            result.Should().NotBeNull();
            Mock.Get(mockRepo).Verify(r => r.UpdateContractStatusAsync(It.IsAny<int>(), It.IsAny<ContractStatus>(), It.IsAny<ContractStatus>()), Times.Exactly(1));
            _mockAuditService.Verify(e => e.TrySendAuditAsync(It.IsAny<AuditModels.Audit>()), Times.Once);
            _mockLogger.Verify();
        }

        [TestMethod]
        public void UpdateContractWithdrawalAsync_TestAsync_ResultContractStatusExceptionExpected()
        {
            // Arrange
            var mockDataModel = Mock.Of<UpdatedContractStatusResponse>();
            var mockRepo = Mock.Of<IContractRepository>(MockBehavior.Strict);
            Mock.Get(mockRepo)
              .Setup(r => r.UpdateContractStatusAsync(It.IsAny<int>(), It.IsAny<ContractStatus>(), It.IsAny<ContractStatus>()))
              .Throws(new ContractStatusException("Contract status is not PublishedByProvider."))
              .Verifiable();

            string actionUrl = "action";
            var mockUriService = Mock.Of<IUriService>(MockBehavior.Strict);
            Mock.Get(mockUriService)
                .Setup(m => m.GetUri(actionUrl))
                .Returns(It.IsAny<Uri>())
                .Verifiable();

            SetupLogger_LogInformationMethod();

            var mockMapper = Mock.Of<IMapper>();

            SetupAuditService_AuditAsyncMethod();

            var contractService = new ContractService(mockRepo, mockMapper, mockUriService, _mockLogger.Object, _mockAuditService.Object, _semaphoreOnEntity);

            var request = GetASingleUpdateContractWithdrawalRequest();

            // Act
            Func<Task> act = async () => await contractService.UpdateContractWithdrawalAsync(request);

            // Assert
            act.Should().Throw<ContractStatusException>();
            Mock.Get(mockRepo).Verify(r => r.UpdateContractStatusAsync(It.IsAny<int>(), It.IsAny<ContractStatus>(), It.IsAny<ContractStatus>()), Times.Once);
            _mockAuditService.Verify(e => e.AuditAsync(It.IsAny<AuditModels.Audit>()), Times.Never);
            _mockLogger.Verify();
        }

        [TestMethod]
        public void UpdateContractWithdrawalAsync_TestAsync_ResultContractNotFoundExceptionExpected()
        {
            // Arrange
            var mockDataModel = Mock.Of<UpdatedContractStatusResponse>();
            var mockRepo = Mock.Of<IContractRepository>(MockBehavior.Strict);
            Mock.Get(mockRepo)
              .Setup(r => r.UpdateContractStatusAsync(It.IsAny<int>(), It.IsAny<ContractStatus>(), It.IsAny<ContractStatus>()))
              .Throws(new ContractNotFoundException("Contract was not found."))
              .Verifiable();

            string actionUrl = "action";
            var mockUriService = Mock.Of<IUriService>(MockBehavior.Strict);
            Mock.Get(mockUriService)
                .Setup(m => m.GetUri(actionUrl))
                .Returns(It.IsAny<Uri>())
                .Verifiable();

            SetupLogger_LogInformationMethod();

            var mockMapper = Mock.Of<IMapper>();

            SetupAuditService_AuditAsyncMethod();

            var contractService = new ContractService(mockRepo, mockMapper, mockUriService, _mockLogger.Object, _mockAuditService.Object, _semaphoreOnEntity);

            var request = GetASingleUpdateContractWithdrawalRequest();

            // Act
            Func<Task> act = async () => await contractService.UpdateContractWithdrawalAsync(request);

            // Assert
            act.Should().Throw<ContractNotFoundException>();
            Mock.Get(mockRepo).Verify(r => r.UpdateContractStatusAsync(It.IsAny<int>(), It.IsAny<ContractStatus>(), It.IsAny<ContractStatus>()), Times.Once);
            _mockAuditService.Verify(e => e.AuditAsync(It.IsAny<AuditModels.Audit>()), Times.Never);
            _mockLogger.Verify();
        }

        [TestMethod]
        public void UpdateContractWithdrawalAsync_TestAsync_ResultGenericExceptionExpected()
        {
            // Arrange
            var mockDataModel = Mock.Of<UpdatedContractStatusResponse>();
            var mockRepo = Mock.Of<IContractRepository>(MockBehavior.Strict);
            Mock.Get(mockRepo)
              .Setup(r => r.UpdateContractStatusAsync(It.IsAny<int>(), It.IsAny<ContractStatus>(), It.IsAny<ContractStatus>()))
              .Throws(new Exception())
              .Verifiable();

            string actionUrl = "action";
            var mockUriService = Mock.Of<IUriService>(MockBehavior.Strict);
            Mock.Get(mockUriService)
                .Setup(m => m.GetUri(actionUrl))
                .Returns(It.IsAny<Uri>())
                .Verifiable();

            SetupLogger_LogInformationMethod();

            var mockMapper = Mock.Of<IMapper>();

            SetupAuditService_AuditAsyncMethod();

            var contractService = new ContractService(mockRepo, mockMapper, mockUriService, _mockLogger.Object, _mockAuditService.Object, _semaphoreOnEntity);

            var request = GetASingleUpdateContractWithdrawalRequest();

            // Act
            Func<Task> act = async () => await contractService.UpdateContractWithdrawalAsync(request);

            // Assert
            act.Should().Throw<Exception>();
            Mock.Get(mockRepo).Verify(r => r.UpdateContractStatusAsync(It.IsAny<int>(), It.IsAny<ContractStatus>(), It.IsAny<ContractStatus>()), Times.Once);
            _mockAuditService.Verify(e => e.AuditAsync(It.IsAny<AuditModels.Audit>()), Times.Never);
            _mockLogger.Verify();
        }

        private ContractRequest GetASingleUpdateLastEmailReminderSentRequest()
        {
            return new ContractRequest() { Id = 1, ContractNumber = "abc", ContractVersion = 1 };
        }

        private UpdateConfirmApprovalRequest GetASingleUpdateConfirmApprovalRequest()
        {
            return new UpdateConfirmApprovalRequest() { Id = 1, ContractNumber = "abc", ContractVersion = 1 };
        }

        private UpdateContractWithdrawalRequest GetASingleUpdateContractWithdrawalRequest()
        {
            return new UpdateContractWithdrawalRequest() { Id = 1, ContractNumber = "abc", ContractVersion = 1, WithdrawalType = ContractStatus.WithdrawnByAgency };
        }

        private void SetUpMockUriService(string actionUrl)
        {
            _mockUriService = Mock.Of<IUriService>(MockBehavior.Strict);
            Mock.Get(_mockUriService)
                .Setup(m => m.GetUri(actionUrl))
                .Returns(new Uri("https://localhost:5001"))
                .Verifiable();
        }

        private void SetupLogger_LogInformationMethod()
        {
            _mockLogger
                .Setup(logger => logger.LogInformation(It.IsAny<string>()))
                .Verifiable();
        }

        private void SetupLogger_LogErrorMethod<TException>()
            where TException : Exception
        {
            _mockLogger
                .Setup(logger => logger.LogError(It.IsAny<TException>(), It.IsAny<string>()))
                .Verifiable();
        }

        private void SetupAuditService_TrySendAuditAsyncMethod()
        {
            _mockAuditService
                .Setup(e => e.TrySendAuditAsync(It.IsAny<AuditModels.Audit>()))
                .Returns(Task.CompletedTask)
                .Verifiable();
        }

        private void SetupAuditService_AuditAsyncMethod()
        {
            _mockAuditService
                .Setup(e => e.AuditAsync(It.IsAny<AuditModels.Audit>()))
                .Returns(Task.CompletedTask)
                .Verifiable();
        }

        private void SetupAuditService_AuditAsyncThrowsExceptionMethod()
        {
            _mockAuditService
                .Setup(e => e.AuditAsync(It.IsAny<AuditModels.Audit>()))
                .Throws(new System.Exception())
                .Verifiable();
        }

        private ContractService GetContractService()
        {
            var contractService = new ContractService(
                _contractRepository,
                _mapper,
                _mockUriService,
                _mockLogger.Object,
                _mockAuditService.Object,
                _semaphoreOnEntity);
            return contractService;
        }

        private void SetupRepository_GetByContractNumberAndVersionAsyncMethod(CreateContractRequest createRequest, DataModels.Contract result)
        {
            Mock.Get(_contractRepository)
                .Setup(p => p.GetByContractNumberAndVersionAsync(createRequest.ContractNumber, createRequest.ContractVersion))
                .Returns(Task.FromResult(result))
                .Verifiable();
        }

        private void SetupRepository_GetByContractNumberAsyncMethod(CreateContractRequest createRequest, IEnumerable<DataModels.Contract> matchedRecords)
        {
            Mock.Get(_contractRepository)
                .Setup(p => p.GetByContractNumberAsync(createRequest.ContractNumber))
                .Returns(Task.FromResult(matchedRecords))
                .Verifiable();
        }

        private void SetupRepository_CreateAsyncMethod(DataModels.Contract contractRecord, Task result)
        {
            Mock.Get(_contractRepository)
                .Setup(p => p.CreateAsync(contractRecord))
                .Returns(result)
                .Verifiable();
        }

        private CreateContractRequest Generate_CreateContractRequest()
        {
            string contractNumber = "TestContractNumber";
            int contractVersion = 1;

            var createRequest = new CreateContractRequest()
            {
                ContractNumber = contractNumber,
                ContractVersion = contractVersion
            };
            return createRequest;
        }

        #endregion


        #region Verify

        private void VerifyAll()
        {
            Mock.Get(_contractRepository).Verify();
            Mock.Get(_mapper).Verify();
            Mock.Get(_mockUriService).Verify();
            _mockLogger.Verify();
            _mockAuditService.Verify();
        }

        #endregion
    }
}