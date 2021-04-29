using System;
using System.Runtime.Serialization;

namespace Pds.Contracts.Data.Common.CustomExceptionHandlers
{
    /// <summary>
    /// Contract not found exception.
    /// </summary>
    [Serializable]
    public class ContractNotFoundException : Exception
    {
        private const string NotPassedIn = "Not passed";

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractNotFoundException"/> class.
        /// </summary>
        /// <param name="message">Exception message text.</param>
        public ContractNotFoundException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractNotFoundException"/> class.
        /// Should be used when a contract cannot be found based on the input.
        /// </summary>
        /// <param name="contractNumber">Contract number.</param>
        /// <param name="versionNumber">Contract version number.</param>
        public ContractNotFoundException(string contractNumber, int? versionNumber)
            : base($"A contract with contract number:{contractNumber ?? NotPassedIn} and version: {versionNumber?.ToString() ?? NotPassedIn} cannot be found")
        {
            ContractNumber = contractNumber;
            VersionNumber = versionNumber;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractNotFoundException"/> class.
        /// </summary>
        /// <param name="info">Serialisation info.</param>
        /// <param name="context">Serialisation context.</param>
        protected ContractNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        /// <summary>
        /// Gets ContractNumber.
        /// </summary>
        public string ContractNumber { get; }

        /// <summary>
        /// Gets VersionNumber.
        /// </summary>
        public int? VersionNumber { get; }

        /// <inheritdoc/>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}