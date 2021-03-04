using MediatR;
using Pds.Audit.Api.Client.Enumerations;
using Pds.Audit.Api.Client.Interfaces;
using Pds.Contracts.Data.Common.Responses;
using Pds.Core.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AuditModels = Pds.Audit.Api.Client.Models;


namespace Pds.Contracts.Data.Services.NotificationHandlers
{
    /// <summary>
    /// Audit Handler.
    /// </summary>
    public class AuditHandler : INotificationHandler<UpdatedContractStatusResponse>
    {
        private const string _appName = "Pds.Contracts.Data.Api";
        private readonly IAuditService _auditService;
        private readonly ILoggerAdapter<AuditHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuditHandler"/> class.
        /// </summary>
        /// <param name="auditService">The audit service used for auditing.</param>
        /// <param name="logger">The logger.</param>
        public AuditHandler(IAuditService auditService, ILoggerAdapter<AuditHandler> logger)
        {
            _auditService = auditService;
            _logger = logger;
        }

        /// <summary>
        /// Try send audit message to Audit API.
        /// </summary>
        /// <param name="notification">UpdatedContractStatusResponse object.</param>
        /// <param name="cancellationToken">Cancellation Token.</param>
        /// <returns>Task.</returns>
        public async Task Handle(UpdatedContractStatusResponse notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"[AuditHandler-Handle] called for contract number: {notification.ContractNumber}, contract version: {notification.ContractVersion}, Contract status: {notification.NewStatus.ToString("G")}.");
            await _auditService.TrySendAuditAsync(GetAudit(notification));
        }

        private AuditModels.Audit GetAudit(UpdatedContractStatusResponse updatedContractStatusResponse)
        {
            string message = $"Contract [{updatedContractStatusResponse.ContractNumber}] Version number [{updatedContractStatusResponse.ContractVersion}] with Id [{updatedContractStatusResponse.Id}] has been {updatedContractStatusResponse.NewStatus}. Additional Information Details: ContractId is: {updatedContractStatusResponse.Id}. Contract Status Before was {updatedContractStatusResponse.Status} . Contract Status After is {updatedContractStatusResponse.NewStatus}";
            return new AuditModels.Audit()
            {
                Action = ActionType.ContractConfirmApproval,
                Severity = SeverityLevel.Information,
                Ukprn = updatedContractStatusResponse.Ukprn,
                Message = message,
                User = $"[{_appName}]"
            };
        }
    }
}
