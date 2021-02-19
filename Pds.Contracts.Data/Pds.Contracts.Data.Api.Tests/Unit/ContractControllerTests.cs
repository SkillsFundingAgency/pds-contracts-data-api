using FluentAssertions;
using FluentAssertions.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Audit.Api.Client.Interfaces;
using Pds.Contracts.Data.Api.Controllers;
using Pds.Contracts.Data.Common.CustomExceptionHandlers;
using Pds.Contracts.Data.Common.Enums;
using Pds.Contracts.Data.Common.Responses;
using Pds.Contracts.Data.Services.Interfaces;
using Pds.Contracts.Data.Services.Models;
using Pds.Contracts.Data.Services.Responses;
using Pds.Core.Logging;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using AuditModels = Pds.Audit.Api.Client.Models;

namespace Pds.Contracts.Data.Api.Tests.Unit
{
    [TestClass, TestCategory("Unit")]
    public class ContractControllerTests
    {
        [TestMethod]
        public async Task GetById_ReturnsSingleContractResultFromContractService()
        {
            // Arrange
            var expected = Mock.Of<Services.Models.Contract>();

            var mockLogger = new Mock<ILoggerAdapter<ContractController>>();

            mockLogger
                .Setup(logger => logger.LogInformation(It.IsAny<string>()))
                .Verifiable();

            var mockContractService = new Mock<IContractService>();

            mockContractService
                .Setup(e => e.GetAsync(1))
                .ReturnsAsync(expected)
                .Verifiable();

            var controller = new ContractController(mockLogger.Object, mockContractService.Object);

            // Act
            var actual = await controller.Get(1);

            // Assert
            actual.Should().BeObjectResult().WithValue(expected);
            mockLogger.Verify();
            mockContractService.Verify();
        }

        [TestMethod]
        public async Task GetById_ReturnsNotFoundResult()
        {
            // Arrange
            var mockLogger = new Mock<ILoggerAdapter<ContractController>>();

            mockLogger
                .Setup(logger => logger.LogInformation(It.IsAny<string>()))
                .Verifiable();

            var mockContractService = new Mock<IContractService>();

            mockContractService
                .Setup(e => e.GetAsync(1))
                .ReturnsAsync(default(Services.Models.Contract))
                .Verifiable();

            var controller = new ContractController(mockLogger.Object, mockContractService.Object);

            // Act
            var actual = await controller.Get(1);

            // Assert
            actual.Result.Should().BeNotFoundResult();
            mockLogger.Verify();
            mockContractService.Verify();
        }

        [TestMethod]
        public async Task GetByContractNumberAndVersion_ReturnsSingleContractResultFromContractServiceAsync()
        {
            // Arrange
            var expected = Mock.Of<Services.Models.Contract>();
            var mockLogger = new Mock<ILoggerAdapter<ContractController>>();
            mockLogger
                .Setup(logger => logger.LogInformation(It.IsAny<string>()))
                .Verifiable();

            var mockContractService = new Mock<IContractService>();
            mockContractService
                .Setup(e => e.GetByContractNumberAndVersionAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(expected)
                .Verifiable();

            var controller = new ContractController(mockLogger.Object, mockContractService.Object);

            // Act
            var actual = await controller.GetByContractNumberAndVersionAsync("some-contract-number", 1);

            // Assert
            actual.Should().BeObjectResult().WithValue(expected);
            mockLogger.Verify();
            mockContractService.Verify();
        }

        [TestMethod]
        public async Task GetByContractNumberAndVersion_ReturnsNotFoundResultAsync()
        {
            // Arrange
            var mockLogger = new Mock<ILoggerAdapter<ContractController>>();
            mockLogger
                .Setup(logger => logger.LogInformation(It.IsAny<string>()))
                .Verifiable();

            var mockContractService = new Mock<IContractService>();
            mockContractService
                .Setup(e => e.GetByContractNumberAndVersionAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(default(Services.Models.Contract))
                .Verifiable();

            var controller = new ContractController(mockLogger.Object, mockContractService.Object);

            // Act
            var actual = await controller.GetByContractNumberAndVersionAsync("invalid-contract-number", 1);

            // Assert
            actual.Result.Should().BeNotFoundResult();
            mockLogger.Verify();
            mockContractService.Verify();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task GetContractReminders_ReturnsExpectedResult()
        {
            // Arrange
            int reminderInterval = 14;
            int pageNumber = 1;
            int pageSize = 1;
            ContractSortOptions sort = ContractSortOptions.LastUpdatedAt;
            SortDirection order = SortDirection.Asc;
            string baseUrl = "https://localhost:5001";
            string requestPath = "/api/contractReminders";
            string queryString = "?reminderInterval=14&page={page}&count=1&sort=LastUpdatedAt&order=Asc";
            string requestQueryString = $"?reminderInterval={reminderInterval}&page={pageNumber}&count={pageSize}&sort=1&order=0";
            string expectedQueryString = $"?reminderInterval={reminderInterval}&page=2&count={pageSize}&sort=LastUpdatedAt&order=Asc";

            var expected = GetExpectedContractReminders();

            var mockLogger = new Mock<ILoggerAdapter<ContractController>>(MockBehavior.Strict);
            mockLogger
                .Setup(logger => logger.LogInformation(It.IsAny<string>()))
                .Verifiable();

            var httpContext = new DefaultHttpContext(); // or mock a `HttpContext`
            httpContext.Request.Path = requestPath;
            httpContext.Request.QueryString = new QueryString(requestQueryString);
            var controllerContext = new ControllerContext()
            {
                HttpContext = httpContext,
            };

            var mockContractService = new Mock<IContractService>(MockBehavior.Strict);
            mockContractService
                .Setup(e => e.GetContractRemindersAsync(reminderInterval, pageNumber, pageSize, sort, order, requestPath + queryString))
                .ReturnsAsync(expected)
                .Verifiable();

            var controller = new ContractController(mockLogger.Object, mockContractService.Object) { ControllerContext = controllerContext };

            // Act
            var actual = await controller.GetContractReminders(reminderInterval, pageNumber, pageSize, sort, order);

            // Assert
            var okResult = actual.Result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeAssignableTo<ContractReminderResponse<IEnumerable<ContractReminderItem>>>().Subject;
            response.Should().BeEquivalentTo(expected);
            response.Paging.NextPageUrl.Should().BeEquivalentTo(baseUrl + requestPath + expectedQueryString);
            mockContractService.Verify();
            mockLogger.Verify();
        }

        [TestMethod]
        public async Task UpdateLastEmailReminderSent_ReturnsOKResultAsync()
        {
            // Arrange
            var expectedDataModel = Mock.Of<Contract>();
            var mockLogger = new Mock<ILoggerAdapter<ContractController>>();
            mockLogger
                .Setup(logger => logger.LogInformation(It.IsAny<string>()))
                .Verifiable();

            var mockContractService = new Mock<IContractService>();
            mockContractService
                .Setup(e => e.UpdateLastEmailReminderSentAndLastUpdatedAtAsync(It.IsAny<UpdateLastEmailReminderSentRequest>()))
                .ReturnsAsync(expectedDataModel)
                .Verifiable();

            var controller = new ContractController(mockLogger.Object, mockContractService.Object);

            // Act
            var actual = await controller.UpdateLastEmailReminderSent(GetUpdateLastEmailReminderSentRequest());

            // Assert
            actual.Should().BeStatusCodeResult().StatusCode.Should().Be((int)HttpStatusCode.OK);
            mockContractService.Verify(e => e.UpdateLastEmailReminderSentAndLastUpdatedAtAsync(It.IsAny<UpdateLastEmailReminderSentRequest>()), Times.Once);
            mockLogger.Verify();
        }

        [TestMethod]
        public async Task UpdateLastEmailReminderSent_ReturnsNotFoundResultAsync()
        {
            // Arrange
            Contract dummyModel = null;
            var expectedDataModel = Mock.Of<Contract>();
            var mockLogger = new Mock<ILoggerAdapter<ContractController>>();
            mockLogger
                .Setup(logger => logger.LogInformation(It.IsAny<string>()))
                .Verifiable();

            var mockContractService = new Mock<IContractService>();
            mockContractService
                .Setup(e => e.UpdateLastEmailReminderSentAndLastUpdatedAtAsync(It.IsAny<UpdateLastEmailReminderSentRequest>()))
                .ReturnsAsync(dummyModel)
                .Verifiable();

            var controller = new ContractController(mockLogger.Object, mockContractService.Object);

            var request = GetUpdateLastEmailReminderSentRequest();

            // Act
            var actual = await controller.UpdateLastEmailReminderSent(request);

            // Assert
            actual.Should().BeStatusCodeResult().StatusCode.Should().Be((int)HttpStatusCode.NotFound);
            mockContractService.Verify(e => e.UpdateLastEmailReminderSentAndLastUpdatedAtAsync(It.IsAny<UpdateLastEmailReminderSentRequest>()), Times.Once);
            mockLogger.Verify();
        }

        [TestMethod]
        public async Task UpdateLastEmailReminderSent_ReturnsBadRequestResultAsync()
        {
            // Arrange
            var expectedDataModel = Mock.Of<Contract>();
            var mockLogger = new Mock<ILoggerAdapter<ContractController>>();
            mockLogger
                .Setup(logger => logger.LogInformation(It.IsAny<string>()))
                .Verifiable();

            var mockContractService = new Mock<IContractService>();
            mockContractService
                .Setup(e => e.UpdateLastEmailReminderSentAndLastUpdatedAtAsync(It.IsAny<UpdateLastEmailReminderSentRequest>()))
                .ReturnsAsync(expectedDataModel)
                .Verifiable();

            var controller = new ContractController(mockLogger.Object, mockContractService.Object);

            controller.ModelState.AddModelError("Id", "Id must be greater than zero");

            var request = GetUpdateLastEmailReminderSentRequest();
            request.Id = 0;

            // Act
            var actual = await controller.UpdateLastEmailReminderSent(request);

            // Assert
            actual.Should().BeStatusCodeResult().StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            mockContractService.Verify(e => e.UpdateLastEmailReminderSentAndLastUpdatedAtAsync(It.IsAny<UpdateLastEmailReminderSentRequest>()), Times.Never);
            mockLogger.Verify();
        }

        [TestMethod]
        public async Task UpdateContractConfirmApproval_ReturnsOKResultAsync()
        {
            // Arrange
            var expectedDataModel = Mock.Of<UpdatedContractStatusResponse>();
            var mockLogger = new Mock<ILoggerAdapter<ContractController>>();
            mockLogger
                .Setup(logger => logger.LogInformation(It.IsAny<string>()))
                .Verifiable();

            var mockContractService = new Mock<IContractService>();
            mockContractService
                .Setup(e => e.UpdateContractConfirmApprovalAsync(It.IsAny<UpdateConfirmApprovalRequest>()))
                .ReturnsAsync(expectedDataModel)
                .Verifiable();

            var controller = new ContractController(mockLogger.Object, mockContractService.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var request = new UpdateConfirmApprovalRequest() { Id = 1, ContractNumber = "abc", ContractVersion = 1 };

            // Act
            var actual = await controller.UpdateContractConfirmApproval(request);

            // Assert
            actual.Should().BeStatusCodeResult().StatusCode.Should().Be((int)HttpStatusCode.OK);
            mockContractService.Verify(e => e.UpdateContractConfirmApprovalAsync(It.IsAny<UpdateConfirmApprovalRequest>()), Times.Once);
            mockLogger.Verify();
        }

        [TestMethod]
        public async Task UpdateContractConfirmApprovalAsync_ReturnsContractPreConditionFailedExceptionResult()
        {
            // Arrange
            var expectedDataModel = Mock.Of<Contract>();
            var mockLogger = new Mock<ILoggerAdapter<ContractController>>();
            mockLogger
                .Setup(logger => logger.LogInformation(It.IsAny<string>()))
                .Verifiable();

            var mockContractService = new Mock<IContractService>();
            mockContractService
                .Setup(e => e.UpdateContractConfirmApprovalAsync(It.IsAny<UpdateConfirmApprovalRequest>()))
                .Throws(new ContractStatusException("Contract status is not ApprovedWaitingConfirmation."))
                .Verifiable();

            var controller = new ContractController(mockLogger.Object, mockContractService.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var request = new UpdateConfirmApprovalRequest() { Id = 1, ContractNumber = "abc", ContractVersion = 1 };

            // Act
            var actual = await controller.UpdateContractConfirmApproval(request);

            // Assert
            actual.Should().BeStatusCodeResult().StatusCode.Should().Be((int)HttpStatusCode.PreconditionFailed);
            mockContractService.Verify(e => e.UpdateContractConfirmApprovalAsync(It.IsAny<UpdateConfirmApprovalRequest>()), Times.Once);
            mockLogger.Verify();
        }

        [TestMethod]
        public async Task UpdateContractConfirmApprovalAsync_ReturnsContractNotFoundExceptionResult()
        {
            // Arrange
            var expectedDataModel = Mock.Of<Contract>();
            var mockLogger = new Mock<ILoggerAdapter<ContractController>>();
            mockLogger
                .Setup(logger => logger.LogInformation(It.IsAny<string>()))
                .Verifiable();

            var mockContractService = new Mock<IContractService>();
            mockContractService
                .Setup(e => e.UpdateContractConfirmApprovalAsync(It.IsAny<UpdateConfirmApprovalRequest>()))
                .Throws(new ContractNotFoundException("Contract was not found."))
                .Verifiable();

            var controller = new ContractController(mockLogger.Object, mockContractService.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var request = new UpdateConfirmApprovalRequest() { Id = 1, ContractNumber = "abc", ContractVersion = 1 };

            // Act
            var actual = await controller.UpdateContractConfirmApproval(request);

            // Assert
            actual.Should().BeStatusCodeResult().StatusCode.Should().Be((int)HttpStatusCode.NotFound);
            mockContractService.Verify(e => e.UpdateContractConfirmApprovalAsync(It.IsAny<UpdateConfirmApprovalRequest>()), Times.Once);
            mockLogger.Verify();
        }

        [TestMethod]
        public async Task UpdateContractConfirmApprovalAsync_ReturnsContractGenericExceptionResult()
        {
            // Arrange
            var expectedDataModel = Mock.Of<Contract>();
            var mockLogger = new Mock<ILoggerAdapter<ContractController>>();
            mockLogger
                .Setup(logger => logger.LogInformation(It.IsAny<string>()))
                .Verifiable();
            mockLogger
                .Setup(logger => logger.LogError(It.IsAny<string>()))
                .Verifiable();

            var mockContractService = new Mock<IContractService>();
            mockContractService
                .Setup(e => e.UpdateContractConfirmApprovalAsync(It.IsAny<UpdateConfirmApprovalRequest>()))
                .Throws(new System.Exception())
                .Verifiable();

            var controller = new ContractController(mockLogger.Object, mockContractService.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var request = new UpdateConfirmApprovalRequest() { Id = 1, ContractNumber = "abc", ContractVersion = 1 };

            // Act
            var actual = await controller.UpdateContractConfirmApproval(request);

            // Assert
            actual.Should().BeStatusCodeResult().StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
            mockContractService.Verify(e => e.UpdateContractConfirmApprovalAsync(It.IsAny<UpdateConfirmApprovalRequest>()), Times.Once);
            mockLogger.Verify();
        }

        [TestMethod]
        public async Task UpdateContractConfirmApprovalAsync_ReturnsContractNotFoundResult()
        {
            // Arrange
            UpdatedContractStatusResponse dummyModel = null;
            var expectedDataModel = Mock.Of<Contract>();
            var mockLogger = new Mock<ILoggerAdapter<ContractController>>();
            mockLogger
                .Setup(logger => logger.LogInformation(It.IsAny<string>()))
                .Verifiable();

            var mockContractService = new Mock<IContractService>();
            mockContractService
                .Setup(e => e.UpdateContractConfirmApprovalAsync(It.IsAny<UpdateConfirmApprovalRequest>()))
                .ReturnsAsync(dummyModel)
                .Verifiable();

            var controller = new ContractController(mockLogger.Object, mockContractService.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var request = new UpdateConfirmApprovalRequest() { Id = 1, ContractNumber = "abc", ContractVersion = 1 };

            // Act
            var actual = await controller.UpdateContractConfirmApproval(request);

            // Assert
            actual.Should().BeStatusCodeResult().StatusCode.Should().Be((int)HttpStatusCode.NotFound);
            mockContractService.Verify(e => e.UpdateContractConfirmApprovalAsync(It.IsAny<UpdateConfirmApprovalRequest>()), Times.Once);
            mockLogger.Verify();
        }

        [TestMethod]
        public async Task UpdateContractConfirmApproval_ReturnsBadRequestResultAsync()
        {
            // Arrange
            var expectedDataModel = Mock.Of<UpdatedContractStatusResponse>();

            var mockLogger = new Mock<ILoggerAdapter<ContractController>>();
            mockLogger
                .Setup(logger => logger.LogInformation(It.IsAny<string>()))
                .Verifiable();

            var mockContractService = new Mock<IContractService>();
            mockContractService
                .Setup(e => e.UpdateContractConfirmApprovalAsync(It.IsAny<UpdateConfirmApprovalRequest>()))
                .ReturnsAsync(expectedDataModel)
                .Verifiable();

            var controller = new ContractController(mockLogger.Object, mockContractService.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            controller.ModelState.AddModelError("Id", "Id must be greater than zero");

            var request = new UpdateConfirmApprovalRequest() { Id = 1, ContractNumber = "abc", ContractVersion = 1 };
            request.Id = 0;

            // Act
            var actual = await controller.UpdateContractConfirmApproval(request);

            // Assert
            actual.Should().BeStatusCodeResult().StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            mockContractService.Verify(e => e.UpdateContractConfirmApprovalAsync(It.IsAny<UpdateConfirmApprovalRequest>()), Times.Never);
            mockLogger.Verify();
        }

        #region Arrange Helpers

        private ControllerContext CreateControllerContext()
        {
            return new ControllerContext() { HttpContext = new DefaultHttpContext() };
        }

        private ContractReminderResponse<IEnumerable<ContractReminderItem>> GetExpectedContractReminders()
        {
            int reminderInterval = 14;
            int pageNumber = 1;
            int pageSize = 1;
            ContractSortOptions sort = ContractSortOptions.LastUpdatedAt;
            SortDirection order = SortDirection.Asc;
            const string contractNumber = "Test-Contract-Number";
            const string title = "Test Title";
            string baseUrl = $"https://localhost:5001";

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
                    NextPageUrl = baseUrl + $"/api/contractReminders?reminderInterval={reminderInterval}&page=2&count={pageSize}&sort={sort}&order={order}",
                    PageSize = pageSize,
                    PreviousPageUrl = string.Empty,
                    TotalCount = 2,
                    TotalPages = 2
                }
            };

            return expected;
        }

        private UpdateLastEmailReminderSentRequest GetUpdateLastEmailReminderSentRequest()
        {
            return new UpdateLastEmailReminderSentRequest() { Id = 1, ContractNumber = "abc", ContractVersion = 1 };
        }

        #endregion Arrange Helpers
    }
}