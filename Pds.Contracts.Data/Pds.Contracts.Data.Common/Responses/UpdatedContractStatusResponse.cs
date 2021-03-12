using MediatR;
using Pds.Audit.Api.Client.Enumerations;
using Pds.Contracts.Data.Common.Enums;

namespace Pds.Contracts.Data.Common.Responses
{
    /// <summary>
    /// Updated Contract Status Response class.
    /// </summary>
    public class UpdatedContractStatusResponse : INotification
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the ukprn.
        /// </summary>
        /// <value>The ukprn.</value>
        public int Ukprn { get; set; }

        /// <summary>
        /// Gets or sets the contract number.
        /// </summary>
        /// <value>The contract number.</value>
        public string ContractNumber { get; set; }

        /// <summary>
        /// Gets or sets the contract version.
        /// </summary>
        /// <value>The contract version.</value>
        public int ContractVersion { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>The status.</value>
        public ContractStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the new status.
        /// </summary>
        /// <value>The status.</value>
        public ContractStatus NewStatus { get; set; }

        /// <summary>
        /// Gets or sets the new Action.
        /// </summary>
        /// <value>The Action Type.</value>
        public ActionType Action { get; set; }

        /// <summary>
        /// Gets or sets the contract amendment type.
        /// </summary>
        /// <value>The contract amendment type enum.</value>
        public ContractAmendmentType AmendmentType { get; set; }
    }
}