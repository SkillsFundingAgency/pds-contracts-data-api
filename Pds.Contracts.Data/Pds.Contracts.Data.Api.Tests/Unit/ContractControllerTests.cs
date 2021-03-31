using FluentAssertions;
using FluentAssertions.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Contracts.Data.Api.Controllers;
using Pds.Contracts.Data.Common.CustomExceptionHandlers;
using Pds.Contracts.Data.Common.Enums;
using Pds.Contracts.Data.Common.Responses;
using Pds.Contracts.Data.Services.Interfaces;
using Pds.Contracts.Data.Services.Models;
using Pds.Contracts.Data.Services.Responses;
using Pds.Core.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Pds.Contracts.Data.Api.Tests.Unit
{
    [TestClass, TestCategory("Unit")]
    public class ContractControllerTests
    {
        private readonly IContractService _contractService
            = Mock.Of<IContractService>(MockBehavior.Strict);

        private readonly ILoggerAdapter<ContractController> _logger
            = Mock.Of<ILoggerAdapter<ContractController>>(MockBehavior.Strict);

        private readonly ProblemDetailsFactory _problemDetailsFactory
            = Mock.Of<ProblemDetailsFactory>(MockBehavior.Strict);

        #region Create

        [TestMethod]
        public async Task Create_Returns_ExpectedResult()
        {
            // Arrange
            var contractRequest = new CreateContractRequest();

            SetupLoggerInfo();

            Mock.Get(_contractService)
                .Setup(e => e.CreateAsync(contractRequest))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var controller = new ContractController(_logger, _contractService);

            // Act
            var result = await controller.CreateContract(contractRequest);

            // Assert
            result.Should().BeStatusCodeResult().WithStatusCode(StatusCodes.Status201Created);

            VerifyAll();
        }

        [TestMethod]
        public async Task Create_InvalidModelState_Returns_BadRequestResult()
        {
            // Arrange
            var contractRequest = new CreateContractRequest();
            string key = "Ukprn";
            string error = "Value must be 8 digits.";

            var validationProblemDetails = new ValidationProblemDetails()
            {
                Detail = "One or more errors with the input",
                Status = StatusCodes.Status400BadRequest
            };

            validationProblemDetails.Errors.Add(key, new string[] { error });

            SetupLoggerInfo();
            SetupLoggerError();
            SetupProblemDetailsFactory(validationProblemDetails);

            var controller = new ContractController(_logger, _contractService);

            controller.ProblemDetailsFactory = _problemDetailsFactory;
            controller.ModelState.AddModelError(key, error);

            // Act
            var result = await controller.CreateContract(contractRequest);

            // Assert
            var badResult = result.Should().BeAssignableTo<ObjectResult>();
            badResult.Subject.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

            badResult.Subject.Value.Should().Be(validationProblemDetails);

            VerifyAll();
        }

        [TestMethod]
        public async Task Create_SignedOn_IsRequired_WhenAmendmentType_IsNotitification_Returns_BadRequestResult()
        {
            // Arrange
            var contractRequest = new CreateContractRequest();
            contractRequest.AmendmentType = ContractAmendmentType.Notfication;
            contractRequest.SignedOn = null;

            var validationProblemDetails = new ValidationProblemDetails()
            {
                Detail = "One or more errors with the input",
                Status = StatusCodes.Status400BadRequest
            };

            SetupLoggerInfo();
            SetupLoggerError();

            SetupProblemDetailsFactory(validationProblemDetails);
            var controller = new ContractController(_logger, _contractService);

            controller.ProblemDetailsFactory = _problemDetailsFactory;

            // Act
            var result = await controller.CreateContract(contractRequest);

            // Assert
            var badResult = result.Should().BeAssignableTo<ObjectResult>();
            badResult.Subject.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

            badResult.Subject.Value.Should().Be(validationProblemDetails);
            VerifyAll();
        }

        [TestMethod]
        public async Task Create_WhenContractAlreadyExists_Returns_409Conflict()
        {
            // Arrange
            var expectedStatus = StatusCodes.Status409Conflict;
            var contractRequest = new CreateContractRequest();
            var problem = new ProblemDetails
            {
                Status = expectedStatus
            };

            SetupLoggerInfo();
            SetupLoggerError<DuplicateContractException>();
            SetupCreateProblemDetails(problem);

            Mock.Get(_contractService)
                .Setup(e => e.CreateAsync(contractRequest))
                .Throws(new DuplicateContractException("test", 1))
                .Verifiable();

            var controller = new ContractController(_logger, _contractService);
            controller.ProblemDetailsFactory = _problemDetailsFactory;

            // Act
            var result = await controller.CreateContract(contractRequest);

            // Assert
            var status = result.Should().BeAssignableTo<ObjectResult>();
            status.Subject.StatusCode.Should().Be(expectedStatus);
            status.Subject.Value.Should().Be(problem);

            VerifyAll();
        }

        [TestMethod]
        public async Task Create_WhenContractAlreadyExists_Returns_412PreconditionFailed()
        {
            // Arrange
            var expectedStatus = StatusCodes.Status412PreconditionFailed;
            var contractRequest = new CreateContractRequest();
            var problem = new ProblemDetails
            {
                Status = expectedStatus
            };

            SetupLoggerInfo();
            SetupLoggerError<ContractWithHigherVersionAlreadyExistsException>();
            SetupCreateProblemDetails(problem);

            Mock.Get(_contractService)
                .Setup(e => e.CreateAsync(contractRequest))
                .Throws(new ContractWithHigherVersionAlreadyExistsException("test", 1))
                .Verifiable();

            var controller = new ContractController(_logger, _contractService);
            controller.ProblemDetailsFactory = _problemDetailsFactory;

            // Act
            var result = await controller.CreateContract(contractRequest);

            // Assert
            var status = result.Should().BeAssignableTo<ObjectResult>();
            status.Subject.StatusCode.Should().Be(StatusCodes.Status412PreconditionFailed);
            status.Subject.Value.Should().Be(problem);

            VerifyAll();
        }

        [TestMethod]
        public async Task Create_WhenSaveToDbFails_Returns_500InternalServerError()
        {
            // Arrange
            var expectedStatus = StatusCodes.Status500InternalServerError;
            var contractRequest = new CreateContractRequest();
            var problem = new ProblemDetails
            {
                Status = expectedStatus
            };

            SetupLoggerInfo();
            SetupLoggerError<Exception>();
            SetupCreateProblemDetails(problem);

            Mock.Get(_contractService)
                .Setup(e => e.CreateAsync(contractRequest))
                .Throws(new Exception())
                .Verifiable();

            var controller = new ContractController(_logger, _contractService);
            controller.ProblemDetailsFactory = _problemDetailsFactory;

            // Act
            var result = await controller.CreateContract(contractRequest);

            // Assert
            var status = result.Should().BeAssignableTo<ObjectResult>();
            status.Subject.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            status.Subject.Value.Should().Be(problem);

            VerifyAll();
        }

        #endregion


        #region GetById

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

        #endregion


        #region GetByContractNumberAndVersion

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

        #endregion


        #region GetContractReminders

        [TestMethod]
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

        #endregion


        #region UpdateLastEmailReminderSent

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

        #endregion


        #region UpdateContractConfirmApproval

        [TestMethod]
        public async Task ConfirmApprovalAsync_ReturnsOKResultAsync()
        {
            // Arrange
            var expectedDataModel = Mock.Of<UpdatedContractStatusResponse>();
            var mockLogger = new Mock<ILoggerAdapter<ContractController>>();
            mockLogger
                .Setup(logger => logger.LogInformation(It.IsAny<string>()))
                .Verifiable();

            var mockContractService = new Mock<IContractService>();
            mockContractService
                .Setup(e => e.ConfirmApprovalAsync(It.IsAny<UpdateConfirmApprovalRequest>()))
                .ReturnsAsync(expectedDataModel)
                .Verifiable();

            var controller = new ContractController(mockLogger.Object, mockContractService.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var request = new UpdateConfirmApprovalRequest() { ContractNumber = "abc", ContractVersion = 1 };

            // Act
            var actual = await controller.ConfirmApprovalAsync(request);

            // Assert
            actual.Should().BeStatusCodeResult().StatusCode.Should().Be((int)HttpStatusCode.OK);
            mockContractService.Verify(e => e.ConfirmApprovalAsync(It.IsAny<UpdateConfirmApprovalRequest>()), Times.Once);
            mockLogger.Verify();
        }

        [TestMethod]
        public async Task ConfirmApprovalAsync_ReturnsContractStatusExceptionResult()
        {
            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status412PreconditionFailed
            };

            SetupLoggerInfo();
            SetupLoggerError();
            SetMockContractService_ConfirmApprove_ContractStatusException();
            SetupCreateProblemDetails(problem);

            var controller = GetContractController();
            controller.ProblemDetailsFactory = _problemDetailsFactory;
            var request = new UpdateConfirmApprovalRequest() { ContractNumber = "Main-0002", ContractVersion = 2, FileName = "dfgdfg.xml" };

            // Act
            var actual = await controller.ConfirmApprovalAsync(request);

            // Assert
            var status = actual.Should().BeAssignableTo<ObjectResult>();
            status.Subject.StatusCode.Should().Be(StatusCodes.Status412PreconditionFailed);
            status.Subject.Value.Should().NotBeNull();
            Mock.Get(_contractService).Verify(e => e.ConfirmApprovalAsync(It.IsAny<UpdateConfirmApprovalRequest>()), Times.Once);
            Mock.Get(_logger).Verify();
        }

        [TestMethod]
        public async Task ConfirmApprovalAsync_ReturnsContractNotFoundExceptionResult()
        {
            // Arrange
            string key = "Id";
            string error = "Contract not found.";
            var validationProblemDetails = new ValidationProblemDetails()
            {
                Detail = "One or more errors with the input",
                Status = StatusCodes.Status404NotFound
            };
            validationProblemDetails.Errors.Add(key, new string[] { error });

            SetupLoggerInfo();
            SetupLoggerError();
            SetMockContractService_ConfirmApprove_ContractNotFoundException();
            SetupProblemDetailsFactory(validationProblemDetails);

            var controller = GetContractController();
            controller.ProblemDetailsFactory = _problemDetailsFactory;
            controller.ModelState.AddModelError(key, error);

            var request = new UpdateConfirmApprovalRequest() { ContractNumber = "Main-0002", ContractVersion = 2, FileName = "sdfgsdfg.xml" };

            // Act
            var actual = await controller.ConfirmApprovalAsync(request);

            // Assert
            var status = actual.Should().BeAssignableTo<ObjectResult>();
            status.Subject.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            status.Subject.Value.Should().NotBeNull();
            Mock.Get(_contractService).Verify(e => e.ConfirmApprovalAsync(It.IsAny<UpdateConfirmApprovalRequest>()), Times.Never);
            Mock.Get(_logger).Verify();
        }

        [TestMethod]
        public async Task ConfirmApprovalAsync_InputInvalid_ReturnsBadRequestResultAsync()
        {
            // Arrange
            SetupLoggerInfo();
            SetupLoggerError();
            var validationProblemDetails = new ValidationProblemDetails()
            {
                Detail = "One or more errors with the input",
                Status = StatusCodes.Status400BadRequest
            };
            SetupProblemDetailsFactory(validationProblemDetails);

            SetMockContractService_ConfirmApprove();
            var controller = GetContractController();
            controller.ProblemDetailsFactory = _problemDetailsFactory;
            controller.ModelState.AddModelError("Id", "Id must be greater than zero");

            var request = new UpdateConfirmApprovalRequest() { ContractNumber = "Main-0002", ContractVersion = 2, FileName = "dfgsdfg.xml" };
            request.ContractNumber = null;

            // Act
            var actual = await controller.ConfirmApprovalAsync(request);

            // Assert
            var status = actual.Should().BeAssignableTo<ObjectResult>();
            status.Subject.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            status.Subject.Value.Should().NotBeNull();
            Mock.Get(_contractService).Verify(e => e.ConfirmApprovalAsync(It.IsAny<UpdateConfirmApprovalRequest>()), Times.Never);
            Mock.Get(_logger).Verify();
        }

        [TestMethod]
        public async Task ConfirmApprovalAsync_ReturnsBlobExceptionResult()
        {
            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError
            };

            SetupLoggerInfo();
            SetupLoggerError();
            SetMockContractService_ConfirmApprove_BlobException();
            SetupCreateProblemDetails(problem);

            var controller = GetContractController();
            controller.ProblemDetailsFactory = _problemDetailsFactory;
            var request = new UpdateConfirmApprovalRequest() { ContractNumber = "Main-0002", ContractVersion = 2, FileName = "dfgdfg.xml" };

            // Act
            var actual = await controller.ConfirmApprovalAsync(request);

            // Assert
            var status = actual.Should().BeAssignableTo<ObjectResult>();
            status.Subject.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            status.Subject.Value.Should().NotBeNull();
            Mock.Get(_contractService).Verify(e => e.ConfirmApprovalAsync(It.IsAny<UpdateConfirmApprovalRequest>()), Times.Once);
            Mock.Get(_logger).Verify();
        }

        [TestMethod]
        public async Task ConfirmApprovalAsync_ReturnsBlobNoContentExceptionResult()
        {
            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError
            };

            SetupLoggerInfo();
            SetupLoggerError();
            SetMockContractService_ConfirmApprove_BlobNoContentException();
            SetupCreateProblemDetails(problem);

            var controller = GetContractController();
            controller.ProblemDetailsFactory = _problemDetailsFactory;
            var request = new UpdateConfirmApprovalRequest() { ContractNumber = "Main-0002", ContractVersion = 2, FileName = "dfgdfg.xml" };

            // Act
            var actual = await controller.ConfirmApprovalAsync(request);

            // Assert
            var status = actual.Should().BeAssignableTo<ObjectResult>();
            status.Subject.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            status.Subject.Value.Should().NotBeNull();
            Mock.Get(_contractService).Verify(e => e.ConfirmApprovalAsync(It.IsAny<UpdateConfirmApprovalRequest>()), Times.Once);
            Mock.Get(_logger).Verify();
        }

        #endregion


        #region UpdateContractWithdrawal

        [TestMethod]
        public async Task UpdateContractWithdrawalAsync_ReturnsOKResultAsync()
        {
            // Arrange
            var expectedStatus = HttpStatusCode.OK;
            var problem = GetProblemDetailsAndSetup(expectedStatus);
            var mockDataModel = Mock.Of<UpdatedContractStatusResponse>(MockBehavior.Strict);

            var expectedDataModel = Mock.Of<Contract>();
            var mockLogger = new Mock<ILoggerAdapter<ContractController>>(MockBehavior.Strict);
            mockLogger
                .Setup(logger => logger.LogInformation(It.IsAny<string>()))
                .Verifiable();

            var mockContractService = new Mock<IContractService>();
            mockContractService
                .Setup(e => e.WithdrawalAsync(It.IsAny<UpdateContractWithdrawalRequest>()))
                .ReturnsAsync(mockDataModel)
                .Verifiable();

            var controller = new ContractController(mockLogger.Object, mockContractService.Object)
            {
                ControllerContext = CreateControllerContext(), ProblemDetailsFactory = _problemDetailsFactory
            };

            var request = new UpdateContractWithdrawalRequest() { ContractNumber = "abc", ContractVersion = 1, WithdrawalType = ContractStatus.WithdrawnByAgency };

            // Act
            var actual = await controller.WithdrawAsync(request);

            // Assert
            actual.Should().BeStatusCodeResult().StatusCode.Should().Be((int)expectedStatus);
            mockContractService.Verify(e => e.WithdrawalAsync(It.IsAny<UpdateContractWithdrawalRequest>()), Times.Once);
            mockLogger.Verify();
        }

        [TestMethod]
        public async Task UpdateContractWithdrawalAsync_ReturnsContractPreConditionFailedExceptionResult()
        {
            // Arrange
            var expectedStatus = HttpStatusCode.PreconditionFailed;
            var problem = GetProblemDetailsAndSetup(expectedStatus);
            var expectedDataModel = Mock.Of<Contract>();
            var mockLogger = new Mock<ILoggerAdapter<ContractController>>(MockBehavior.Strict);
            mockLogger
                .Setup(logger => logger.LogInformation(It.IsAny<string>()))
                .Verifiable();
            mockLogger
             .Setup(logger => logger.LogError(It.IsAny<Exception>(), It.IsAny<string>()))
             .Verifiable();

            var mockContractService = new Mock<IContractService>();
            mockContractService
                .Setup(e => e.WithdrawalAsync(It.IsAny<UpdateContractWithdrawalRequest>()))
                .Throws(new ContractStatusException("Contract status is not PublishedToProvider."))
                .Verifiable();

            var controller = new ContractController(mockLogger.Object, mockContractService.Object)
            {
                ControllerContext = CreateControllerContext(), ProblemDetailsFactory = _problemDetailsFactory
            };

            var request = new UpdateContractWithdrawalRequest() { ContractNumber = "abc", ContractVersion = 1, WithdrawalType = ContractStatus.WithdrawnByAgency };

            // Act
            var result = await controller.WithdrawAsync(request);

            // Assert
            var status = result.Should().BeAssignableTo<ObjectResult>();
            status.Subject.StatusCode.Should().Be((int)expectedStatus);
            status.Subject.Value.Should().Be(problem);
            mockContractService.Verify(e => e.WithdrawalAsync(It.IsAny<UpdateContractWithdrawalRequest>()), Times.Once);
            mockLogger.Verify();
        }

        [TestMethod]
        public async Task UpdateContractWithdrawalAsync_ReturnsContractNotFoundExceptionResult()
        {
            // Arrange
            var expectedStatus = HttpStatusCode.NotFound;
            var problem = GetProblemDetailsAndSetup(expectedStatus);
            var expectedDataModel = Mock.Of<Contract>();
            var mockLogger = new Mock<ILoggerAdapter<ContractController>>(MockBehavior.Strict);
            mockLogger
                .Setup(logger => logger.LogInformation(It.IsAny<string>()))
                .Verifiable();
            mockLogger
             .Setup(logger => logger.LogError(It.IsAny<Exception>(), It.IsAny<string>()))
             .Verifiable();

            var mockContractService = new Mock<IContractService>();
            mockContractService
                .Setup(e => e.WithdrawalAsync(It.IsAny<UpdateContractWithdrawalRequest>()))
                .Throws(new ContractNotFoundException("Contract was not found."))
                .Verifiable();

            var controller = new ContractController(mockLogger.Object, mockContractService.Object)
            {
                ControllerContext = CreateControllerContext(),
                ProblemDetailsFactory = _problemDetailsFactory
            };

            var request = new UpdateContractWithdrawalRequest() { ContractNumber = "abc", ContractVersion = 1, WithdrawalType = ContractStatus.WithdrawnByAgency };

            // Act
            var result = await controller.WithdrawAsync(request);

            // Assert
            var status = result.Should().BeAssignableTo<ObjectResult>();
            status.Subject.StatusCode.Should().Be((int)expectedStatus);
            status.Subject.Value.Should().Be(problem);
            mockContractService.Verify(e => e.WithdrawalAsync(It.IsAny<UpdateContractWithdrawalRequest>()), Times.Once);
            mockLogger.Verify();
        }

        [TestMethod]
        public async Task UpdateContractWithdrawalAsync_ReturnsContractGenericExceptionResult()
        {
            // Arrange
            var expectedStatus = HttpStatusCode.InternalServerError;
            var problem = GetProblemDetailsAndSetup(expectedStatus);
            var expectedDataModel = Mock.Of<Contract>();
            var mockLogger = new Mock<ILoggerAdapter<ContractController>>(MockBehavior.Strict);
            mockLogger
                .Setup(logger => logger.LogInformation(It.IsAny<string>()))
                .Verifiable();
            mockLogger
                .Setup(logger => logger.LogError(It.IsAny<Exception>(), It.IsAny<string>()))
                .Verifiable();

            var mockContractService = new Mock<IContractService>();
            mockContractService
                .Setup(e => e.WithdrawalAsync(It.IsAny<UpdateContractWithdrawalRequest>()))
                .Throws(new System.Exception())
                .Verifiable();

            var controller = new ContractController(mockLogger.Object, mockContractService.Object)
            {
                ControllerContext = CreateControllerContext(),
                ProblemDetailsFactory = _problemDetailsFactory
            };

            var request = new UpdateContractWithdrawalRequest() { ContractNumber = "abc", ContractVersion = 1 };

            // Act
            var result = await controller.WithdrawAsync(request);

            // Assert
            var status = result.Should().BeAssignableTo<ObjectResult>();
            status.Subject.StatusCode.Should().Be((int)expectedStatus);
            status.Subject.Value.Should().Be(problem);
            mockContractService.Verify(e => e.WithdrawalAsync(It.IsAny<UpdateContractWithdrawalRequest>()), Times.Once);
            mockLogger.Verify();
        }

        [TestMethod]
        public async Task UpdateContractWithdrawalAsync_ReturnsContractNotFoundResult()
        {
            // Arrange
            var expectedStatus = HttpStatusCode.NotFound;
            var problem = GetProblemDetailsAndSetup(expectedStatus);
            UpdatedContractStatusResponse dummyModel = null;
            var expectedDataModel = Mock.Of<Contract>();
            var mockLogger = new Mock<ILoggerAdapter<ContractController>>(MockBehavior.Strict);
            mockLogger
                .Setup(logger => logger.LogInformation(It.IsAny<string>()))
                .Verifiable();

            var mockContractService = new Mock<IContractService>();
            mockContractService
                .Setup(e => e.WithdrawalAsync(It.IsAny<UpdateContractWithdrawalRequest>()))
                .ReturnsAsync(dummyModel)
                .Verifiable();

            var controller = new ContractController(mockLogger.Object, mockContractService.Object)
            {
                ControllerContext = CreateControllerContext(),
                ProblemDetailsFactory = _problemDetailsFactory
            };

            var request = new UpdateContractWithdrawalRequest() { ContractNumber = "abc", ContractVersion = 1, WithdrawalType = ContractStatus.WithdrawnByAgency };

            // Act
            var result = await controller.WithdrawAsync(request);

            // Assert
            var status = result.Should().BeAssignableTo<ObjectResult>();
            status.Subject.StatusCode.Should().Be((int)expectedStatus);
            status.Subject.Value.Should().Be(problem);
            mockContractService.Verify(e => e.WithdrawalAsync(It.IsAny<UpdateContractWithdrawalRequest>()), Times.Once);
            mockLogger.Verify();
        }

        [TestMethod]
        public async Task UpdateContractWithdrawalAsync_ReturnsBadRequestResultAsync()
        {
            // Arrange
            var expectedStatus = HttpStatusCode.BadRequest;
            var validationProblemDetails = new ValidationProblemDetails()
            {
                Detail = "One or more errors with the input",
                Status = (int)expectedStatus
            };
            SetupProblemDetailsFactory(validationProblemDetails);

            var expectedDataModel = Mock.Of<UpdatedContractStatusResponse>();

            var mockLogger = new Mock<ILoggerAdapter<ContractController>>(MockBehavior.Strict);
            mockLogger
                .Setup(logger => logger.LogInformation(It.IsAny<string>()))
                .Verifiable();

            var mockContractService = new Mock<IContractService>();
            mockContractService
                .Setup(e => e.WithdrawalAsync(It.IsAny<UpdateContractWithdrawalRequest>()))
                .ReturnsAsync(expectedDataModel)
                .Verifiable();

            var controller = new ContractController(mockLogger.Object, mockContractService.Object)
            {
                ControllerContext = CreateControllerContext()
            };
            controller.ProblemDetailsFactory = _problemDetailsFactory;

            controller.ModelState.AddModelError("Id", "Id must be greater than zero");

            var request = new UpdateContractWithdrawalRequest() { ContractNumber = "abc", ContractVersion = 1, WithdrawalType = ContractStatus.WithdrawnByAgency };
            request.ContractNumber = null;

            // Act
            var result = await controller.WithdrawAsync(request);

            // Assert
            var status = result.Should().BeAssignableTo<ObjectResult>();
            status.Subject.StatusCode.Should().Be((int)expectedStatus);
            status.Subject.Value.Should().Be(validationProblemDetails);
            mockContractService.Verify(e => e.WithdrawalAsync(It.IsAny<UpdateContractWithdrawalRequest>()), Times.Never);
            mockLogger.Verify();
        }

        [TestMethod]
        public async Task UpdateContractWithdrawalAsync_ReturnsContractUpdateConcurrencyException()
        {
            // Arrange
            var expectedStatus = HttpStatusCode.Conflict;
            var problem = GetProblemDetailsAndSetup(expectedStatus);
            var expectedDataModel = Mock.Of<Contract>();
            var mockLogger = new Mock<ILoggerAdapter<ContractController>>(MockBehavior.Strict);
            mockLogger
                .Setup(logger => logger.LogInformation(It.IsAny<string>()))
                .Verifiable();
            mockLogger
             .Setup(logger => logger.LogError(It.IsAny<Exception>(), It.IsAny<string>()))
             .Verifiable();

            var mockContractService = new Mock<IContractService>();
            mockContractService
                .Setup(e => e.WithdrawalAsync(It.IsAny<UpdateContractWithdrawalRequest>()))
                .Throws(new ContractUpdateConcurrencyException(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<ContractStatus>()))
                .Verifiable();

            var controller = new ContractController(mockLogger.Object, mockContractService.Object)
            {
                ControllerContext = CreateControllerContext(),
                ProblemDetailsFactory = _problemDetailsFactory
            };

            var request = new UpdateContractWithdrawalRequest() { ContractNumber = "abc", ContractVersion = 1, WithdrawalType = ContractStatus.WithdrawnByAgency };

            // Act
            var result = await controller.WithdrawAsync(request);

            // Assert
            var status = result.Should().BeAssignableTo<ObjectResult>();
            status.Subject.StatusCode.Should().Be((int)expectedStatus);
            status.Subject.Value.Should().Be(problem);
            mockContractService.Verify(e => e.WithdrawalAsync(It.IsAny<UpdateContractWithdrawalRequest>()), Times.Once);
            mockLogger.Verify();
        }

        #endregion UpdateContractWithdrawal


        #region ManualApprove tests.

        [TestMethod]
        public async Task ManualApprove_ReturnsOKResultAsync()
        {
            // Arrange
            var expectedStatus = HttpStatusCode.OK;
            var problem = GetProblemDetailsAndSetup(expectedStatus);
            SetMockLogger();

            SetMockContractService_ManualApprove();

            var controller = GetContractController();

            var request = GetContractRequest();

            // Act
            var result = await controller.ManualApprove(request);

            // Assert
            result.Should().BeStatusCodeResult().WithStatusCode((int)expectedStatus);
            Mock.Get(_contractService).Verify(e => e.ApproveManuallyAsync(It.IsAny<ContractRequest>()), Times.Once);
            Mock.Get(_logger).Verify();
        }

        [TestMethod]
        public async Task ManualApprove_InputInvalid_InvalidContractRequestExceptionAsync()
        {
            // Arrange
            var expectedStatus = HttpStatusCode.BadRequest;
            var problem = GetProblemDetailsAndSetup(expectedStatus);
            SetMockLogger_Error();
            SetMockContractService_ManualApprove_InvalidContractRequestException();
            var controller = GetContractController();

            var request = GetContractRequest();
            request.ContractNumber = "xyz";

            // Act
            var result = await controller.ManualApprove(request);

            // Assert
            var status = result.Should().BeAssignableTo<ObjectResult>();
            status.Subject.StatusCode.Should().Be((int)expectedStatus);
            status.Subject.Value.Should().Be(problem);
            Mock.Get(_contractService).Verify(e => e.ApproveManuallyAsync(It.IsAny<ContractRequest>()), Times.Once);
            Mock.Get(_logger).Verify();
        }

        [TestMethod]
        public async Task ManualApprove_ReturnsContractNotFoundExceptionResult()
        {
            // Arrange
            var expectedStatus = HttpStatusCode.NotFound;
            var problem = GetProblemDetailsAndSetup(expectedStatus);
            SetMockLogger_Error();
            SetMockContractService_ManualApprove_ContractNotFoundException();
            var controller = GetContractController();
            var request = GetContractRequest();

            // Act
            var result = await controller.ManualApprove(request);

            // Assert
            var status = result.Should().BeAssignableTo<ObjectResult>();
            status.Subject.StatusCode.Should().Be((int)expectedStatus);
            status.Subject.Value.Should().Be(problem);
            Mock.Get(_contractService).Verify(e => e.ApproveManuallyAsync(It.IsAny<ContractRequest>()), Times.Once);
            Mock.Get(_logger).Verify();
        }

        [TestMethod]
        public async Task ManualApprove_ReturnsContractExpectationFailedExceptionResult()
        {
            // Arrange
            var expectedStatus = HttpStatusCode.NotFound;
            var problem = GetProblemDetailsAndSetup(expectedStatus);
            SetMockLogger_Error();
            SetMockContractService_ManualApprove_ContractExpectationFailedException();
            var controller = GetContractController();
            var request = GetContractRequest();

            // Act
            var result = await controller.ManualApprove(request);

            // Assert
            var status = result.Should().BeAssignableTo<ObjectResult>();
            status.Subject.StatusCode.Should().Be((int)expectedStatus);
            status.Subject.Value.Should().Be(problem);
            Mock.Get(_contractService).Verify(e => e.ApproveManuallyAsync(It.IsAny<ContractRequest>()), Times.Once);
            Mock.Get(_logger).Verify();
        }

        [TestMethod]
        public async Task ManualApprove_ReturnsContractStatusExceptionResult()
        {
            // Arrange
            var expectedStatus = HttpStatusCode.PreconditionFailed;
            var problem = GetProblemDetailsAndSetup(expectedStatus);
            SetMockLogger_Error();
            SetMockContractService_ManualApprove_ContractStatusException();
            var controller = GetContractController();
            var request = GetContractRequest();

            // Act
            var result = await controller.ManualApprove(request);

            // Assert
            var status = result.Should().BeAssignableTo<ObjectResult>();
            status.Subject.StatusCode.Should().Be((int)expectedStatus);
            status.Subject.Value.Should().Be(problem);
            Mock.Get(_contractService).Verify(e => e.ApproveManuallyAsync(It.IsAny<ContractRequest>()), Times.Once);
            Mock.Get(_logger).Verify();
        }

        [TestMethod]
        public async Task ManualApprove_ReturnsGeneralExceptionResult()
        {
            // Arrange
            var expectedStatus = HttpStatusCode.InternalServerError;
            var problem = GetProblemDetailsAndSetup(expectedStatus);
            SetMockLogger_Error();
            SetMockContractService_ManualApprove_GeneralException();
            var controller = GetContractController();
            var request = GetContractRequest();

            // Act
            var result = await controller.ManualApprove(request);

            // Assert
            var status = result.Should().BeAssignableTo<ObjectResult>();
            status.Subject.StatusCode.Should().Be((int)expectedStatus);
            status.Subject.Value.Should().Be(problem);
            Mock.Get(_contractService).Verify(e => e.ApproveManuallyAsync(It.IsAny<ContractRequest>()), Times.Once);
            Mock.Get(_logger).Verify();
        }

        [TestMethod]
        public async Task ManualApprove_ReturnsContractUpdateConcurrencyException()
        {
            // Arrange
            var expectedStatus = HttpStatusCode.Conflict;
            var problem = GetProblemDetailsAndSetup(expectedStatus);
            SetMockLogger_Error();

            SetMockContractService_ManualApprove_ContractUpdateConcurrencyException();

            var controller = GetContractController();
            var request = GetContractRequest();

            // Act
            var result = await controller.ManualApprove(request);

            // Assert
            var status = result.Should().BeAssignableTo<ObjectResult>();
            status.Subject.StatusCode.Should().Be((int)expectedStatus);
            status.Subject.Value.Should().Be(problem);
            Mock.Get(_contractService).Verify(e => e.ApproveManuallyAsync(It.IsAny<ContractRequest>()), Times.Once);
            Mock.Get(_logger).Verify();
        }

        #endregion ManualApprove tests.


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

        private void SetupLoggerInfo()
        {
            Mock.Get(_logger)
                .Setup(logger => logger.LogInformation(It.IsAny<string>()))
                .Verifiable();
        }

        private void SetupLoggerError()
        {
            Mock.Get(_logger)
                .Setup(logger => logger.LogError(It.IsAny<string>()))
                .Verifiable();
        }

        /// <summary>
        /// Adds a setup to _logger to ensure the given exception type is raised.
        /// </summary>
        /// <typeparam name="TException">The exception type.</typeparam>
        private void SetupLoggerError<TException>()
            where TException : Exception
        {
            Mock.Get(_logger)
                .Setup(logger => logger.LogError(It.IsAny<TException>(), It.IsAny<string>()))
                .Verifiable();
        }

        private void SetupProblemDetailsFactory(ValidationProblemDetails validationProblemDetails)
        {
            Mock.Get(_problemDetailsFactory)
                .Setup(p => p.CreateValidationProblemDetails(
                    It.IsAny<DefaultHttpContext>(),
                    It.IsAny<ModelStateDictionary>(),
                    null,
                    null,
                    null,
                    null,
                    null))
                .Returns(validationProblemDetails);
        }

        private void SetupCreateProblemDetails(ProblemDetails problemDetails)
        {
            Mock.Get(_problemDetailsFactory)
                .Setup(p => p.CreateProblemDetails(It.IsAny<HttpContext>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(problemDetails);
        }

        private ContractRequest GetContractRequest()
        {
            return new ContractRequest() { ContractNumber = "abc", ContractVersion = 1 };
        }

        private ContractController GetContractController()
        {
            return new ContractController(_logger, _contractService)
            {
                ControllerContext = CreateControllerContext(),
                ProblemDetailsFactory = _problemDetailsFactory
            };
        }

        private void SetMockLogger()
        {
            Mock.Get(_logger)
                .Setup(logger => logger.LogInformation(It.IsAny<string>()))
                .Verifiable();
        }

        private void SetMockLogger_Error()
        {
            Mock.Get(_logger)
                .Setup(logger => logger.LogInformation(It.IsAny<string>()))
                .Verifiable();
            Mock.Get(_logger)
                .Setup(logger => logger.LogError(It.IsAny<Exception>(), It.IsAny<string>()))
                .Verifiable();
        }

        private void SetMockContractService_ConfirmApprove()
        {
            var mockDataModel = Mock.Of<UpdatedContractStatusResponse>(MockBehavior.Strict);

            Mock.Get(_contractService)
                .Setup(e => e.ConfirmApprovalAsync(It.IsAny<UpdateConfirmApprovalRequest>()))
                .ReturnsAsync(mockDataModel)
                .Verifiable();
        }

        private void SetMockContractService_ConfirmApprove_ContractNotFoundException()
        {
            Mock.Get(_contractService)
                .Setup(e => e.ConfirmApprovalAsync(It.IsAny<UpdateConfirmApprovalRequest>()))
                .Throws(new ContractNotFoundException("Contract was not found."))
                .Verifiable();
        }

        private void SetMockContractService_ConfirmApprove_ContractStatusException()
        {
            Mock.Get(_contractService)
                .Setup(e => e.ConfirmApprovalAsync(It.IsAny<UpdateConfirmApprovalRequest>()))
                .Throws(new ContractStatusException("Contract status is not ApprovedWaitingConfirmation."))
                .Verifiable();
        }

        private void SetMockContractService_ConfirmApprove_BlobException()
        {
            Mock.Get(_contractService)
                .Setup(e => e.ConfirmApprovalAsync(It.IsAny<UpdateConfirmApprovalRequest>()))
                .Throws(new BlobException("Contract status is not ApprovedWaitingConfirmation."))
                .Verifiable();
        }

        private void SetMockContractService_ConfirmApprove_BlobNoContentException()
        {
            Mock.Get(_contractService)
                .Setup(e => e.ConfirmApprovalAsync(It.IsAny<UpdateConfirmApprovalRequest>()))
                .Throws(new BlobNoContentException("Contract status is not ApprovedWaitingConfirmation."))
                .Verifiable();
        }

        private void SetMockContractService_ManualApprove()
        {
            var mockDataModel = Mock.Of<UpdatedContractStatusResponse>(MockBehavior.Strict);

            Mock.Get(_contractService)
                .Setup(e => e.ApproveManuallyAsync(It.IsAny<ContractRequest>()))
                .ReturnsAsync(mockDataModel)
                .Verifiable();
        }

        private void SetMockContractService_ManualApprove_InvalidContractRequestException()
        {
            Mock.Get(_contractService)
                .Setup(e => e.ApproveManuallyAsync(It.IsAny<ContractRequest>()))
                .Throws(new InvalidContractRequestException("abc", 1))
                .Verifiable();
        }

        private void SetMockContractService_ManualApprove_ContractStatusException()
        {
            Mock.Get(_contractService)
                .Setup(e => e.ApproveManuallyAsync(It.IsAny<ContractRequest>()))
                .Throws(new ContractStatusException("Contract status is not ApprovedWaitingConfirmation."))
                .Verifiable();
        }

        private void SetMockContractService_ManualApprove_ContractNotFoundException()
        {
            Mock.Get(_contractService)
                .Setup(e => e.ApproveManuallyAsync(It.IsAny<ContractRequest>()))
                .Throws(new ContractNotFoundException("Contract was not found."))
                .Verifiable();
        }

        private void SetMockContractService_ManualApprove_ContractExpectationFailedException()
        {
            Mock.Get(_contractService)
                .Setup(e => e.ApproveManuallyAsync(It.IsAny<ContractRequest>()))
                .Throws(new ContractExpectationFailedException("abc", 1, "null"))
                .Verifiable();
        }

        private void SetMockContractService_ManualApprove_GeneralException()
        {
            Mock.Get(_contractService)
                 .Setup(e => e.ApproveManuallyAsync(It.IsAny<ContractRequest>()))
                 .Throws(new System.Exception())
                 .Verifiable();
        }

        private void SetMockContractService_ManualApprove_ContractUpdateConcurrencyException()
        {
            Mock.Get(_contractService)
                 .Setup(e => e.ApproveManuallyAsync(It.IsAny<ContractRequest>()))
                 .Throws(new ContractUpdateConcurrencyException(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<ContractStatus>()))
                 .Verifiable();
        }

        private ProblemDetails GetProblemDetailsAndSetup(HttpStatusCode expectedStatus)
        {
            var problem = new ProblemDetails
            {
                Status = (int)expectedStatus
            };

            SetupCreateProblemDetails(problem);

            return problem;
        }


        #endregion Arrange Helpers


        #region Verify Helpers

        private void VerifyAll()
        {
            Mock.Get(_contractService).VerifyAll();
            Mock.Get(_logger).VerifyAll();
            Mock.Get(_problemDetailsFactory).VerifyAll();
        }

        #endregion
    }
}