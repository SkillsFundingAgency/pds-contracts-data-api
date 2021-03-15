using Pds.Contracts.Data.Services.Interfaces;
using Pds.Contracts.Data.Services.Models;

namespace Pds.Contracts.Data.Services.Responses
{
    /// <summary>
    /// ApiResponse.
    /// </summary>
    /// <typeparam name="T">Response data type.</typeparam>
    public class ContractReminderResponse<T> : IContractReminderResponse<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContractReminderResponse{T}"/> class.
        /// </summary>
        /// <param name="data">data.</param>
        public ContractReminderResponse(T data)
        {
            Contracts = data;
        }

        /// <inheritdoc/>
        public T Contracts { get; set; }

        /// <inheritdoc/>
        public Metadata Paging { get; set; }
    }
}