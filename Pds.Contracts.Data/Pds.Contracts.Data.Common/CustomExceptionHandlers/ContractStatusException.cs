using System;

namespace Pds.Contracts.Data.Common.CustomExceptionHandlers
{
    /// <summary>
    /// Contract status exception.
    /// </summary>
    public class ContractStatusException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContractStatusException"/> class.
        /// </summary>
        /// <param name="message">Message to set in base exception.</param>
        public ContractStatusException(string message) : base(message)
        {
        }
    }
}
