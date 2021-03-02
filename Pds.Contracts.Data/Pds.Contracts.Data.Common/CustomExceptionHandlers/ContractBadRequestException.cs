using System;

namespace Pds.Contracts.Data.Common.CustomExceptionHandlers
{
    /// <summary>
    /// Contract bad request exception.
    /// </summary>
    public class ContractBadRequestException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContractBadRequestException"/> class.
        /// </summary>
        /// <param name="message">Message to set in base exception.</param>
        public ContractBadRequestException(string message) : base(message)
        {
        }
    }
}
