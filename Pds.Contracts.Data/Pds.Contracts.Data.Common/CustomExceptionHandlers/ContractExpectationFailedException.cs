using System;
using System.Runtime.Serialization;

namespace Pds.Contracts.Data.Common.CustomExceptionHandlers
{
    /// <summary>
    /// Contract expectation failed exception.
    /// </summary>
    [Serializable]
    public class ContractExpectationFailedException : Exception
    {
        private const string NotPassedIn = "Not passed";

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractExpectationFailedException"/> class.
        /// </summary>
        /// <param name="contractNumber">Contract number.</param>
        /// <param name="versionNumber">Contract version number.</param>
        /// <param name="failedExpectation">Failed validation expression.</param>
        public ContractExpectationFailedException(string contractNumber, int? versionNumber, string failedExpectation)
            : base($"Expectation: {failedExpectation} failed in contract with contract number:{contractNumber ?? NotPassedIn} and version: {versionNumber?.ToString() ?? NotPassedIn}.")
        {
            ContractNumber = contractNumber;
            VersionNumber = versionNumber;
            FailedExpectation = failedExpectation;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractExpectationFailedException"/> class.
        /// </summary>
        /// <param name="info">Serialisation info.</param>
        /// <param name="context">Serialisation context.</param>
        protected ContractExpectationFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
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

        /// <summary>
        /// Gets Predicate.
        /// </summary>
        public string FailedExpectation { get; }

        /// <inheritdoc/>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}