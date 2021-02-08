using Pds.Contracts.Data.Services.Models;

namespace Pds.Contracts.Data.Services.Interfaces
{
    /// <summary>
    /// Contract reminder response interface.
    /// </summary>
    /// <typeparam name="T">Type.</typeparam>
    public interface IContractReminderResponse<T>
    {
        /// <summary>
        /// Gets or sets data.
        /// </summary>
        T Contracts { get; set; }

        /// <summary>
        /// Gets or sets meta.
        /// </summary>
        Metadata Paging { get; set; }
    }
}