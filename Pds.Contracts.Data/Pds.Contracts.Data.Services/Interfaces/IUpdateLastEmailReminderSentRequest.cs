using System.ComponentModel.DataAnnotations;

namespace Pds.Contracts.Data.Services.Interfaces
{
    /// <summary>
    /// Request model for updating LastEmailReminderSent and LastUpdatedAt.
    /// </summary>
    public interface IUpdateLastEmailReminderSentRequest
    {
        /// <summary>
        /// Gets or sets the contract number.
        /// </summary>
        /// <value>
        /// The contract number.
        /// </value>
        [Required]
        string ContractNumber { get; set; }

        /// <summary>
        /// Gets or sets the contract version.
        /// </summary>
        /// <value>
        /// The contract version.
        /// </value>
        [Required]
        int ContractVersion { get; set; }
    }
}
