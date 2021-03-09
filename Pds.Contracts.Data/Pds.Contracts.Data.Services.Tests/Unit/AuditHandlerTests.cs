using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Audit.Api.Client.Interfaces;
using Pds.Contracts.Data.Common.Enums;
using Pds.Contracts.Data.Common.Responses;
using Pds.Contracts.Data.Services.NotificationHandlers;
using Pds.Core.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AuditModels = Pds.Audit.Api.Client.Models;


namespace Pds.Contracts.Data.Services.Tests.Unit
{
    [TestClass, TestCategory("Unit")]
    public class AuditHandlerTests
    {
        private readonly IAuditService _mockAuditService
            = Mock.Of<IAuditService>(MockBehavior.Strict);

        private readonly ILoggerAdapter<AuditHandler> _mockLogger
            = Mock.Of<ILoggerAdapter<AuditHandler>>(MockBehavior.Strict);

        [TestMethod]
        public async Task Handle_ReturnsExpectedResult()
        {
            //Arrange
            SetupLogger_LogInformationMethod();
            SetupAuditService_TrySendAuditAsyncMethod();
            var auditHandler = GetAuditHandler();


            //Act
            await auditHandler.Handle(GetUpdatedContractStatusResponse(), default);

            Mock.Get(_mockAuditService).Verify(e => e.TrySendAuditAsync(It.IsAny<AuditModels.Audit>()), Times.Once);
            Mock.Get(_mockLogger).Verify();
        }

        #region Arrange Helpers

        private void SetupLogger_LogInformationMethod()
        {
            Mock.Get(_mockLogger)
                .Setup(logger => logger.LogInformation(It.IsAny<string>()))
                .Verifiable();
        }

        private void SetupAuditService_TrySendAuditAsyncMethod()
        {
            Mock.Get(_mockAuditService)
                .Setup(e => e.TrySendAuditAsync(It.IsAny<AuditModels.Audit>()))
                .Returns(Task.CompletedTask)
                .Verifiable();
        }

        private AuditHandler GetAuditHandler()
        {
            return new AuditHandler(
                _mockAuditService,
                _mockLogger);
        }

        private UpdatedContractStatusResponse GetUpdatedContractStatusResponse()
        {
            return new UpdatedContractStatusResponse()
            {
                Id = 1,
                ContractNumber = "abc",
                ContractVersion = 1,
                NewStatus = ContractStatus.Approved,
                Status = ContractStatus.PublishedToProvider,
                Ukprn = 12345678
            };
        }

        #endregion Arrange Helpers

    }
}
