using System.ComponentModel.DataAnnotations;

namespace Pds.Contracts.Data.Services.Models
{
    /// <summary>
    /// Request confirmation approval object.
    /// </summary>
    public class UpdateConfirmApprovalRequest
    {
        /// <summary>
        /// Gets or sets the contract Id.
        /// </summary>
        /// <value>
        /// The contract Id.
        /// </value>
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please enter a value greater than zero.")]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the contract number.
        /// </summary>
        /// <value>
        /// The contract number.
        /// </value>
        [Required]
        [StringLength(20, MinimumLength = 1)]
        public string ContractNumber { get; set; }

        /// <summary>
        /// Gets or sets the contract version.
        /// </summary>
        /// <value>
        /// The contract version.
        /// </value>
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please enter a value greater than zero.")]
        public int ContractVersion { get; set; }
    }
}
