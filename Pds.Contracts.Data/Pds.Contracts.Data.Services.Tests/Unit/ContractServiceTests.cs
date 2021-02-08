using AutoMapper;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Contracts.Data.Common.Enums;
using Pds.Contracts.Data.Repository.DataModels;
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
using System.Linq.Expressions;
using System.Threading.Tasks;

using DataModels = Pds.Contracts.Data.Repository.DataModels;

namespace Pds.Contracts.Data.Services.Tests.Unit
{
    [TestClass, TestCategory("Unit")]
    public class ContractServiceTests
    {
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

            var contractService = new ContractService(mockRepo, mockMapper, _mockUriService, _mockLogger.Object);

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

            var contractService = new ContractService(mockRepo, mockMapper, _mockUriService, _mockLogger.Object);

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

            var contractService = new ContractService(mockRepo, mockMapper, _mockUriService, _mockLogger.Object);

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
            var expectedDummyServiceModel = new ContractReminderResponse<IEnumerable<ContractReminderItem>>(expectedDummyReminderItems);
            expectedDummyServiceModel.Paging = expectedDummyMetadata;

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

            var contractService = new ContractService(mockRepo, mockMapper, _mockUriService, _mockLogger.Object);

            // Act
            var actual = await contractService.GetContractRemindersAsync(reminderInterval, pageNumber, pageSize, sort, order, routeTemplateURL);

            // Assert
            actual.Should().BeEquivalentTo(expectedDummyServiceModel);
            Mock.Get(mockRepo).Verify(r => r.GetContractRemindersAsync(currentDateTimeMinusNumberOfDays, pageNumber, pageSize, sort, order), Times.Once);
            Mock.Get(mockMapper).Verify(m => m.Map<IEnumerable<Models.ContractReminderItem>>(new List<DataModels.Contract>()), Times.Once);
            _mockLogger.Verify();
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
    }
}