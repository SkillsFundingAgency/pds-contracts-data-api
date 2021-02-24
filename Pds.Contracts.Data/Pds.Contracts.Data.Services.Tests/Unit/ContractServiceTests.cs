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
        private Mock<IAuditService> _mockAuditService;

        private Mock<ILoggerAdapter<ContractService>> _mockLogger;

        private IUriService _mockUriService = null;

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

            SetMockLogger();

            var mockMapper = Mock.Of<IMapper>();
            Mock.Get(mockMapper)
                .Setup(m => m.Map<Models.Contract>(It.IsAny<DataModels.Contract>()))
                .Returns(expectedServiceModel)
                .Verifiable();

            MockAuditService();

            var contractService = new ContractService(mockRepo, mockMapper, _mockUriService, _mockLogger.Object, _mockAuditService.Object);

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

            SetMockLogger();

            var mockMapper = Mock.Of<IMapper>();
            Mock.Get(mockMapper)
                .Setup(m => m.Map<IList<Models.Contract>>(It.IsAny<IQueryable<DataModels.Contract>>()))
                .Returns(expectedServiceModelCollection)
                .Verifiable();

            MockAuditService();

            var contractService = new ContractService(mockRepo, mockMapper, _mockUriService, _mockLogger.Object, _mockAuditService.Object);

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

            SetMockLogger();

            var mockMapper = Mock.Of<IMapper>();
            Mock.Get(mockMapper)
                .Setup(m => m.Map<Models.Contract>(It.IsAny<DataModels.Contract>()))
                .Returns(expectedServiceModel)
                .Verifiable();

            MockAuditService();

            var contractService = new ContractService(mockRepo, mockMapper, _mockUriService, _mockLogger.Object, _mockAuditService.Object);

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
            SetMockLogger();

            var mockMapper = Mock.Of<IMapper>();
            Mock.Get(mockMapper)
                .Setup(m => m.Map<IEnumerable<Models.ContractReminderItem>>(new List<DataModels.Contract>()))
                .Returns(expectedDummyReminderItems)
                .Verifiable();

            MockAuditService();

            var contractService = new ContractService(mockRepo, mockMapper, _mockUriService, _mockLogger.Object, _mockAuditService.Object);

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

            _mockLogger = new Mock<ILoggerAdapter<ContractService>>();

            var mockMapper = Mock.Of<IMapper>();
            Mock.Get(mockMapper)
                .Setup(m => m.Map<Models.Contract>(It.IsAny<DataModels.Contract>()))
                .Returns(dummyServiceModel)
                .Verifiable();

            MockAuditService();

            var contractService = new ContractService(mockRepo, mockMapper, mockUriService, _mockLogger.Object, _mockAuditService.Object);

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

            SetMockLogger();

            var mockMapper = Mock.Of<IMapper>();

            MockAuditService();

            var contractService = new ContractService(mockRepo, mockMapper, mockUriService, _mockLogger.Object, _mockAuditService.Object);

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

            SetMockLogger();

            var mockMapper = Mock.Of<IMapper>();

            MockAuditService();

            var contractService = new ContractService(mockRepo, mockMapper, mockUriService, _mockLogger.Object, _mockAuditService.Object);

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

            SetMockLogger();

            var mockMapper = Mock.Of<IMapper>();

            MockAuditService();

            var contractService = new ContractService(mockRepo, mockMapper, mockUriService, _mockLogger.Object, _mockAuditService.Object);

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

            SetMockLogger();

            var mockMapper = Mock.Of<IMapper>();

            MockAuditService();

            var contractService = new ContractService(mockRepo, mockMapper, mockUriService, _mockLogger.Object, _mockAuditService.Object);

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

            SetMockLogger();

            _mockLogger
                .Setup(logger => logger.LogError(It.IsAny<string>()))
                .Verifiable();

            var mockMapper = Mock.Of<IMapper>();

            MockAuditError();

            var contractService = new ContractService(mockRepo, mockMapper, mockUriService, _mockLogger.Object, _mockAuditService.Object);

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

            SetMockLogger();

            var mockMapper = Mock.Of<IMapper>();

            MockAuditService();

            var contractService = new ContractService(mockRepo, mockMapper, mockUriService, _mockLogger.Object, _mockAuditService.Object);

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

            SetMockLogger();

            var mockMapper = Mock.Of<IMapper>();

            MockAuditService();

            var contractService = new ContractService(mockRepo, mockMapper, mockUriService, _mockLogger.Object, _mockAuditService.Object);

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

            SetMockLogger();

            var mockMapper = Mock.Of<IMapper>();

            MockAuditService();

            var contractService = new ContractService(mockRepo, mockMapper, mockUriService, _mockLogger.Object, _mockAuditService.Object);

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

            SetMockLogger();

            var mockMapper = Mock.Of<IMapper>();

            MockAuditService();

            var contractService = new ContractService(mockRepo, mockMapper, mockUriService, _mockLogger.Object, _mockAuditService.Object);

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

        private void SetMockLogger()
        {
            _mockLogger = new Mock<ILoggerAdapter<ContractService>>();

            _mockLogger
                .Setup(logger => logger.LogInformation(It.IsAny<string>()))
                .Verifiable();
        }

        private void MockAuditService()
        {
            _mockAuditService = new Mock<IAuditService>();

            _mockAuditService
                .Setup(e => e.TrySendAuditAsync(It.IsAny<AuditModels.Audit>()))
                .Returns(Task.CompletedTask)
                .Verifiable();
        }

        private void MockAuditError()
        {
            _mockAuditService = new Mock<IAuditService>();

            _mockAuditService
                .Setup(e => e.AuditAsync(It.IsAny<AuditModels.Audit>()))
                .Throws(new System.Exception())
                .Verifiable();
        }
    }
}