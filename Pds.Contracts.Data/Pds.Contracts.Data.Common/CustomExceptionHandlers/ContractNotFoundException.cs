using System;

namespace Pds.Contracts.Data.Common.CustomExceptionHandlers
{
    /// <summary>
    /// Contract not found exception.
    /// </summary>
    public class ContractNotFoundException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContractNotFoundException"/> class.
        /// </summary>
        /// <param name="message">Message to set in base exception.</param>
        public ContractNotFoundException(string message) : base(message)
        {
        }
    }
}
