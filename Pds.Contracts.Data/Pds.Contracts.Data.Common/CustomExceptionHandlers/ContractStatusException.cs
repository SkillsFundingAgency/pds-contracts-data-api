using Pds.Contracts.Data.Common.Enums;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Pds.Contracts.Data.Common.CustomExceptionHandlers
{
    /// <summary>
    /// Contract status exception.
    /// </summary>
    [Serializable]
    public class ContractStatusException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContractStatusException"/> class.
        /// </summary>
        public ContractStatusException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractStatusException"/> class.
        /// </summary>
        /// <param name="message">Message to set in base exception.</param>
        public ContractStatusException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractStatusException"/> class.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="innerException">Inner exception.</param>
        public ContractStatusException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractStatusException"/> class.
        /// </summary>
        /// <param name="info">Serialisation info.</param>
        /// <param name="context">Serialisation context.</param>
        protected ContractStatusException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        /// <summary>
        /// Gets or sets current status of contract.
        /// </summary>
        public ContractStatus CurrentStatus { get; set; }

        /// <summary>
        /// Gets or sets proposed new contract status.
        /// </summary>
        public ContractStatus NewStatus { get; set; }

        /// <summary>
        /// Gets or sets list of allowed statuses.
        /// </summary>
        public IList<ContractStatus> AllowedStatuses { get; set; } = new List<ContractStatus>();

        /// <inheritdoc/>
        public override string Message => $"{base.Message}, Current status is: {CurrentStatus}, trying to set to new status {NewStatus} {(AllowedStatuses.Count > 0 ? $"whereas allowed status are {string.Join(",", AllowedStatuses)}" : string.Empty)}";

        /// <inheritdoc/>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}
