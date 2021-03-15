using Pds.Contracts.Data.Common.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Pds.Contracts.Data.Services.Models
{
    /// <summary>
    /// Contract item service model, that will be exposed as schema from API.
    /// </summary>
    public class ContractReminderItem
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the ukprn.
        /// </summary>
        /// <value>
        /// The ukprn.
        /// </value>
        public int Ukprn { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>The title.</value>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the contract number.
        /// </summary>
        /// <value>The contract number.</value>
        [Required]
        public string ContractNumber { get; set; }

        /// <summary>
        /// Gets or sets the contract version.
        /// </summary>
        /// <value>The contract version.</value>
        [Required]
        public int ContractVersion { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>The status.</value>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ContractStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the type of the funding.
        /// </summary>
        /// <value>The type of the funding.</value>
        public ContractFundingType FundingType { get; set; }
    }
}