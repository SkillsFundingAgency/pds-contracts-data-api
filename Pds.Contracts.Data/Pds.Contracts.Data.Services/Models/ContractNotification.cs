using Pds.Contracts.Data.Common.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Pds.Contracts.Data.Services.Models
{
    /// <summary>
    /// Contract Notification object sent to the Notification service bus topic for sending emails.
    /// </summary>
    public class ContractNotification
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

        /// <summary>
        /// Gets or sets the current status of the contract.
        /// </summary>
        [Required]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ContractStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the UKPRN assocaited with the contract.
        /// </summary>
        // TODO : Config can UKPRN be less than 8 digits?
        [Required]
        [RegularExpression("^[0-9]{8}$", ErrorMessage = "UKPRN should consist of 8 digits.")]
        public int UKPRN { get; set; }
    }
}
