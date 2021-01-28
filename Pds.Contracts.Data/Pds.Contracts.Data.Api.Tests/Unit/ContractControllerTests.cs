using FluentAssertions;
using FluentAssertions.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Contracts.Data.Api.Controllers;
using Pds.Contracts.Data.Services.Interfaces;
using Pds.Contracts.Data.Services.Models;
using Pds.Contracts.Data.Services.Models.Enums;
using Pds.Core.Logging;
using System.Collections.Generic;
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
            var expected = ExpectedContractReminders();

            var mockLogger = new Mock<ILoggerAdapter<ContractController>>();

            mockLogger
                .Setup(logger => logger.LogInformation(It.IsAny<string>()))
                .Verifiable();

            var mockExampleService = new Mock<IContractService>();
            var controller = new ContractController(mockLogger.Object, mockExampleService.Object);

            // Act
            var actual = await controller.GetContractReminders();

            // Assert
            actual.Value.Should().BeEquivalentTo(expected);
            mockLogger.Verify();
        }

        #region Arrange Helpers

        private ContractReminders ExpectedContractReminders()
        {
            ContractReminders rtn = new ContractReminders();

            Contract one = new Contract()
            {
                Title = "ESF SSW contract variation for Humber LEP version 5",
                ContractNumber = "ESIF-5014",
                ContractVersion = 5,
                Status = ContractStatus.Approved,
                FundingType = ContractFundingType.Esf
            };

            Contract two = new Contract()
            {
                Title = "ESF SSW contract variation for Humber LEP version 5",
                ContractNumber = "ESIF-5014",
                ContractVersion = 5,
                Status = ContractStatus.Approved,
                FundingType = ContractFundingType.Esf
            };

            IList<Contract> contractList = new List<Contract>() { one, two };

            rtn.Contracts = contractList;

            rtn.CurrentPage = 1;
            rtn.PageCount = 10;
            rtn.SortedBy = ContractSortOptions.CreatedAt |
                ContractSortOptions.Value |
                ContractSortOptions.ContractVersion;
            rtn.TotalCount = 200;

            return rtn;
        }

        #endregion

    }
}