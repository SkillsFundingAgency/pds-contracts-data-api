using AutoMapper;
using MediatR;
using Pds.Audit.Api.Client.Enumerations;
using Pds.Contracts.Data.Common.Enums;
using Pds.Contracts.Data.Common.Responses;
using Pds.Contracts.Data.Services.Interfaces;
using Pds.Contracts.Data.Services.Models;
using Pds.Core.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pds.Contracts.Data.Services.NotificationHandlers
{
    /// <summary>
    /// Contract Status Change Handler.
    /// </summary>
    public class ContractStatusChangeHandler : INotificationHandler<UpdatedContractStatusResponse>
    {
        private readonly IMessagePublisher _messagePublisher;
        private readonly IMapper _mapper;
        private readonly ILoggerAdapter<ContractStatusChangeHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractStatusChangeHandler"/> class.
        /// </summary>
        /// <param name="messagePublisher">The message publisher used for sending messages to notification service bus topic.</param>
        /// <param name="mapper">Automapper instance.</param>
        /// <param name="logger">The logger.</param>
        public ContractStatusChangeHandler(IMessagePublisher messagePublisher, IMapper mapper, ILoggerAdapter<ContractStatusChangeHandler> logger)
        {
            _messagePublisher = messagePublisher;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Contract Status Change Handler handled when UpdatedContractStatusResponse published.
        /// </summary>
        /// <param name="notification">Updated Contract Status Response object.</param>
        /// <param name="cancellationToken">Cancellation Token.</param>
        /// <returns>Task.</returns>
        public async Task Handle(UpdatedContractStatusResponse notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"[ContractStatusChangeHandler-Handle] called for contract number: {notification.ContractNumber}, contract version: {notification.ContractVersion}, Contract status: {notification.NewStatus.ToString("G")}.");
            await SendNotification(notification);
        }

        private async Task SendNotification(UpdatedContractStatusResponse updatedContractStatusResponse)
        {
            var notificationMessage = _mapper.Map<ContractNotification>(updatedContractStatusResponse);

            IDictionary<string, string> properties = new Dictionary<string, string>();
            switch (updatedContractStatusResponse.Action)
            {
                case ActionType.ContractConfirmApproval:
                case ActionType.ContractManualApproval:
                    properties.Add("Status", MessageStatus.Approved.ToString("G"));
                    break;
                case ActionType.ContractWithdrawal:
                    properties.Add("Status", MessageStatus.Withdrawn.ToString("G"));
                    break;
                case ActionType.ContractCreated:
                    if (notificationMessage.Status == ContractStatus.Approved)
                    {
                        properties.Add("Status", MessageStatus.ReadyToReview.ToString("G"));
                    }
                    else if (notificationMessage.Status == ContractStatus.PublishedToProvider)
                    {
                        properties.Add("Status", MessageStatus.ReadyToSign.ToString("G"));
                    }

                    break;
                default:
                    break;
            }

            if (properties.Count > 0)
            {
                _logger.LogInformation($"[SendNotification] Action type: {updatedContractStatusResponse.Action}, contract number: {notificationMessage.ContractNumber}, contract version: {notificationMessage.ContractVersion}, Contract is in {notificationMessage.Status.ToString("G")} status, Notification sending.");
                await _messagePublisher.PublisherAsync<ContractNotification>(notificationMessage, properties);
                _logger.LogInformation($"[SendNotification] Action type: {updatedContractStatusResponse.Action}, contract number: {notificationMessage.ContractNumber}, contract version: {notificationMessage.ContractVersion}, Contract is in {notificationMessage.Status.ToString("G")} status, Notification sent.");
            }
            else
            {
                _logger.LogInformation($"[SendNotification] Action type: {updatedContractStatusResponse.Action}, contract number: {notificationMessage.ContractNumber}, contract version: {notificationMessage.ContractVersion}, Contract is in {notificationMessage.Status.ToString("G")} status, Notification will not be sent.");
            }
        }
    }
}
