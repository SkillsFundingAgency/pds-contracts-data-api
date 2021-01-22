using FluentAssertions;
using FluentAssertions.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Contracts.Data.Api.Controllers;
using Pds.Contracts.Data.Services.Interfaces;
using Pds.Core.Logging;
using System.Threading.Tasks;

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
        public void GetByContractNumberAndVersion_ReturnsSingleContractResultFromContractService()
        {
            // Arrange
            var expected = Mock.Of<Services.Models.Contract>();
            var mockLogger = new Mock<ILoggerAdapter<ContractController>>();
            mockLogger
                .Setup(logger => logger.LogInformation(It.IsAny<string>()))
                .Verifiable();

            var mockContractService = new Mock<IContractService>();
            mockContractService
                .Setup(e => e.GetByContractNumberAndVersion(It.IsAny<string>(), It.IsAny<int>()))
                .Returns(expected)
                .Verifiable();

            var controller = new ContractController(mockLogger.Object, mockContractService.Object);

            // Act
            var actual = controller.GetByContractNumberAndVersion("some-contract-number", 1);

            // Assert
            actual.Should().BeObjectResult().WithValue(expected);
            mockLogger.Verify();
            mockContractService.Verify();
        }

        [TestMethod]
        public void GetByContractNumberAndVersion_ReturnsNotFoundResult()
        {
            // Arrange
            var mockLogger = new Mock<ILoggerAdapter<ContractController>>();
            mockLogger
                .Setup(logger => logger.LogInformation(It.IsAny<string>()))
                .Verifiable();

            var mockContractService = new Mock<IContractService>();
            mockContractService
                .Setup(e => e.GetByContractNumberAndVersion(It.IsAny<string>(), It.IsAny<int>()))
                .Returns(default(Services.Models.Contract))
                .Verifiable();

            var controller = new ContractController(mockLogger.Object, mockContractService.Object);

            // Act
            var actual = controller.GetByContractNumberAndVersion("invalid-contract-number", 1);

            // Assert
            actual.Result.Should().BeNotFoundResult();
            mockLogger.Verify();
            mockContractService.Verify();
        }
    }
}