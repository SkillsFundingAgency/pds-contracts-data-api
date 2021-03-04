using AutoMapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Contracts.Data.Common.Enums;
using Pds.Contracts.Data.Common.Responses;
using Pds.Contracts.Data.Services.Interfaces;
using Pds.Contracts.Data.Services.Models;
using Pds.Contracts.Data.Services.NotificationHandlers;
using Pds.Core.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.Contracts.Data.Services.Tests.Unit
{
    [TestClass, TestCategory("Unit")]
    public class ContractStatusChangeHandlerTests
    {
        private readonly IMapper _mockMapper
            = Mock.Of<IMapper>(MockBehavior.Strict);

        private readonly IMessagePublisher _mockMessagePublisher
            = Mock.Of<IMessagePublisher>(MockBehavior.Strict);

        private readonly ILoggerAdapter<ContractStatusChangeHandler> _mockLogger
            = Mock.Of<ILoggerAdapter<ContractStatusChangeHandler>>(MockBehavior.Strict);

        [TestMethod]
        public async Task Handle_ReturnsExpectedResult()
        {
            //Arrange
            SetupLogger_LogInformationMethod();
            SetupMessagePublisher_PublisherAsync();
            SetupMapper_PublisherAsync();
            var contractStatusChangeHandler = GetContractStatusChangeHandler();


            //Act
            await contractStatusChangeHandler.Handle(GetUpdatedContractStatusResponse(), default);

            Mock.Get(_mockMessagePublisher).Verify(e => e.PublisherAsync<ContractNotification>(It.IsAny<ContractNotification>(), It.IsAny<IDictionary<string, string>>()), Times.Once);
            Mock.Get(_mockMapper).Verify(m => m.Map<ContractNotification>(It.IsAny<UpdatedContractStatusResponse>()), Times.Once);
            Mock.Get(_mockLogger).Verify();
        }


        #region Arrange Helpers

        private void SetupLogger_LogInformationMethod()
        {
            Mock.Get(_mockLogger)
                .Setup(logger => logger.LogInformation(It.IsAny<string>()))
                .Verifiable();
        }

        private void SetupMessagePublisher_PublisherAsync()
        {
            Mock.Get(_mockMessagePublisher)
                .Setup(e => e.PublisherAsync<ContractNotification>(It.IsAny<ContractNotification>(), It.IsAny<IDictionary<string, string>>()))
                .Returns(Task.CompletedTask)
                .Verifiable();
        }

        private void SetupMapper_PublisherAsync()
        {
            ContractNotification expectedModel = GetContractNotification();
            Mock.Get(_mockMapper)
                .Setup(m => m.Map<ContractNotification>(It.IsAny<UpdatedContractStatusResponse>()))
                .Returns(expectedModel)
                .Verifiable();
        }

        private ContractStatusChangeHandler GetContractStatusChangeHandler()
        {
            return new ContractStatusChangeHandler(
                _mockMessagePublisher,
                _mockMapper,
                _mockLogger);
        }

        private UpdatedContractStatusResponse GetUpdatedContractStatusResponse()
        {
            return new UpdatedContractStatusResponse()
            {
                Id = 1,
                ContractNumber = "abc",
                ContractVersion = 1,
                NewStatus = (int)ContractStatus.Approved,
                Status = (int)ContractStatus.PublishedToProvider,
                Ukprn = 12345678
            };
        }

        private ContractNotification GetContractNotification()
        {
            return new ContractNotification()
            {
                Id = 1,
                ContractNumber = "abc",
                ContractVersion = 1,
                Status = ContractStatus.Approved,
                UKPRN = 12345678
            };
        }

        #endregion Arrange Helpers

    }
}
