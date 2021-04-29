using AutoMapper;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Audit.Api.Client.Enumerations;
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
        [DataRow(ActionType.ContractManualApproval, ContractStatus.Approved, MessageStatus.Approved)]
        [DataRow(ActionType.ContractConfirmApproval, ContractStatus.Approved, MessageStatus.Approved)]
        [DataRow(ActionType.ContractWithdrawal, ContractStatus.WithdrawnByAgency, MessageStatus.Withdrawn)]
        [DataRow(ActionType.ContractWithdrawal, ContractStatus.WithdrawnByProvider, MessageStatus.Withdrawn)]
        [DataRow(ActionType.ContractCreated, ContractStatus.Approved, MessageStatus.ReadyToReview)]
        [DataRow(ActionType.ContractCreated, ContractStatus.PublishedToProvider, MessageStatus.ReadyToSign)]
        public async Task Handle_ReturnsExpectedResult(ActionType action, ContractStatus contractStatus, MessageStatus messageStatus)
        {
            //Arrange
            SetupLogger_LogInformationMethod();
            SetupMessagePublisher_PublisherAsync(messageStatus);
            SetupMapper_PublisherAsync(contractStatus);
            var contractStatusChangeHandler = GetContractStatusChangeHandler();


            //Act
            await contractStatusChangeHandler.Handle(GetUpdatedContractStatusResponse(action, contractStatus), default);

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

        private void SetupMessagePublisher_PublisherAsync(MessageStatus messageStatus)
        {
            Mock.Get(_mockMessagePublisher)
                .Setup(e => e.PublisherAsync<ContractNotification>(It.IsAny<ContractNotification>(), It.IsAny<IDictionary<string, string>>()))
                .Callback((ContractNotification n, IDictionary<string, string> properties) =>
                {
                    properties.Count.Should().Be(1);
                    properties.ContainsKey("Status").Should().BeTrue();
                    properties["Status"].Should().BeEquivalentTo(messageStatus.ToString("G"));
                })
                .Returns(Task.CompletedTask)
                .Verifiable();
        }

        private void SetupMapper_PublisherAsync(ContractStatus notificationStatus)
        {
            ContractNotification expectedModel = GetContractNotification();
            expectedModel.Status = notificationStatus;
            Mock.Get(_mockMapper)
                .Setup(m => m.Map<ContractNotification>(It.IsAny<UpdatedContractStatusResponse>()))
                .Returns(expectedModel)
                .Verifiable();
        }

        private void ForMemberFix(UpdatedContractStatusResponse src, ContractNotification dest)
        {
            dest.Status = src.NewStatus;
        }

        private ContractStatusChangeHandler GetContractStatusChangeHandler()
        {
            return new ContractStatusChangeHandler(
                _mockMessagePublisher,
                _mockMapper,
                _mockLogger);
        }

        private UpdatedContractStatusResponse GetUpdatedContractStatusResponse(ActionType action, ContractStatus contractStatus)
        {
            return new UpdatedContractStatusResponse()
            {
                Id = 1,
                ContractNumber = "abc",
                ContractVersion = 1,
                NewStatus = contractStatus,
                Status = ContractStatus.PublishedToProvider,
                Ukprn = 12345678,
                Action = action
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
