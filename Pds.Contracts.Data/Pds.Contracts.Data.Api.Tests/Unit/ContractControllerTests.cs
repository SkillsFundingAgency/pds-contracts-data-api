using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Contracts.Data.Api.Controllers;
using Pds.Contracts.Data.Services.Interfaces;
using Pds.Core.Logging;
using System.Threading.Tasks;

namespace Pds.Contracts.Data.Api.Tests.Unit
{
    [TestClass]
    public class ContractControllerTests
    {
        [TestMethod, TestCategory("Unit")]
        public async Task GetById_ReturnsSingleContractResultFromContractService()
        {
            // Arrange
            var expected = Mock.Of<Services.Models.Contract>();

            var mockLogger = new Mock<ILoggerAdapter<ContractController>>();

            mockLogger
                .Setup(logger => logger.LogInformation(It.IsAny<string>()))
                .Verifiable();

            var mockExampleService = new Mock<IContractService>();

            mockExampleService
                .Setup(e => e.GetAsync(1))
                .ReturnsAsync(expected)
                .Verifiable();

            var controller = new ContractController(mockLogger.Object, mockExampleService.Object);

            // Act
            var actual = await controller.Get(1);

            // Assert
            actual.Should().Be(expected);
            mockLogger.Verify();
            mockExampleService.Verify();
        }
    }
}