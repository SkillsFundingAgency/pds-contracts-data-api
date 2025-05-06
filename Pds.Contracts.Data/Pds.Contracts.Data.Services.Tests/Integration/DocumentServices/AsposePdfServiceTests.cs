using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Contracts.Data.Common.Enums;
using Pds.Contracts.Data.Services.DocumentServices;
using Pds.Core.Logging;
using System;
using System.Runtime.Versioning;

namespace Pds.Contracts.Data.Services.Tests.Integration.DocumentServices
{
    [TestClass]
    public class AsposePdfServiceTests
    {
        private Mock<ILoggerAdapter<AsposeDocumentManagementService>> _mockLogger;

        #region AddSignedDocumentPage
        [SupportedOSPlatform("windows")]
        [TestMethod, TestCategory("Integration"), TestCategory("CoreIntegration")]
        public void AddSignedDocumentPage_ManuallySigned()
        {
            // Arrange
            SetMockLogger();
            SetAsposeLicense();
            var signer = $"hand and approved by DfE";
            var systemUnderTest = new AsposeDocumentManagementContractService(_mockLogger.Object);

            var fileContents = ContractContentBuilder.GetDocumentBytes("12345678_CityDeals-0002_v1.pdf");

            // Act
            var actual = systemUnderTest.AddSignedDocumentPage(fileContents, "12345678_CityDeals-0002_v1", signer, new DateTime(2021, 10, 18, 18, 55, 10), true, ContractFundingType.Levy, null);

            // Assert
            actual.ShouldHaveSignedPage("12345678_CityDeals-0002_v1", signer, new DateTime(2021, 10, 18, 18, 55, 10), true, "in-memory-file.pdf", ContractFundingType.Levy, null);
        }

        [SupportedOSPlatform("windows")]
        [TestMethod, TestCategory("Integration"), TestCategory("CoreIntegration")]
        public void AddSignedDocumentPage()
        {
            // Arrange
            SetMockLogger();
            SetAsposeLicense();
            var signer = $"hand and approved by DfE";
            var systemUnderTest = new AsposeDocumentManagementContractService(_mockLogger.Object);

            var fileContents = ContractContentBuilder.GetDocumentBytes("12345678_CityDeals-0002_v1.pdf");
            var principalId = "UserID";

            // Act
            var actual = systemUnderTest.AddSignedDocumentPage(fileContents, "12345678_CityDeals-0002_v1", signer, new DateTime(2021, 10, 18, 18, 55, 10), false, ContractFundingType.Esf, principalId);

            // Assert
            actual.ShouldHaveSignedPage("12345678_CityDeals-0002_v1", signer, new DateTime(2021, 10, 18, 18, 55, 10), false, "in-memory-file.pdf", ContractFundingType.Esf, principalId);
        }

        #endregion


        private void SetMockLogger()
        {
            _mockLogger = new Mock<ILoggerAdapter<AsposeDocumentManagementService>>(MockBehavior.Strict);

            _mockLogger
                .Setup(logger => logger.LogInformation(It.IsAny<string>()))
                .Verifiable();
        }

        private void SetAsposeLicense()
        {
            var mockServiceCollection = new Mock<IServiceCollection>();
            AsposeLicenceManagement.AddAsposeLicense(mockServiceCollection.Object);
        }
    }
}
