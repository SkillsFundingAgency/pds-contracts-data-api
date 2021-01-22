using Pds.Contracts.Data.Services.Models.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Pds.Contracts.Data.Services.Models
{
    /// <summary>
    /// Contract item service model, that will be exposed as schema from API.
    /// </summary>
    public class Contract
    {
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
        /// <value>
        /// The title.
        /// </value>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the contract number.
        /// </summary>
        /// <value>
        /// The contract number.
        /// </value>
        [Required]
        public string ContractNumber { get; set; }

        /// <summary>
        /// Gets or sets the parent contract number.
        /// </summary>
        /// <value>
        /// The parent contract number.
        /// </value>
        public string ParentContractNumber { get; set; }


        /// <summary>
        /// Gets or sets the contract version.
        /// </summary>
        /// <value>
        /// The contract version.
        /// </value>
        [Required]
        public int ContractVersion { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public decimal Value { get; set; }


        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ContractStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the type of the funding.
        /// </summary>
        /// <value>
        /// The type of the funding.
        /// </value>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ContractFundingType FundingType { get; set; }

        /// <summary>
        /// Gets or sets the display name of the signed by.
        /// </summary>
        /// <value>
        /// The display name of the signed by.
        /// </value>
        public string SignedByDisplayName { get; set; }

        /// <summary>
        /// Gets or sets the signed by.
        /// </summary>
        /// <value>
        /// The signed by.
        /// </value>
        public string SignedBy { get; set; }

        /// <summary>
        /// Gets or sets the signed on.
        /// </summary>
        /// <value>
        /// The signed on.
        /// </value>
        public DateTime? SignedOn { get; set; }

        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        /// <value>
        /// The start date.
        /// </value>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date.
        /// </summary>
        /// <value>
        /// The end date.
        /// </value>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Gets or sets the type of the amendment.
        /// </summary>
        /// <value>
        /// The type of the amendment.
        /// </value>
        public int AmendmentType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [was manually approved].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [was manually approved]; otherwise, <c>false</c>.
        /// </value>
        public bool WasManuallyApproved { get; set; }

        /// <summary>
        /// Gets or sets the last email reminder sent.
        /// </summary>
        /// <value>
        /// The last email reminder sent.
        /// </value>
        public DateTime? LastEmailReminderSent { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has notification been read.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has notification been read; otherwise, <c>false</c>.
        /// </value>
        public bool HasNotificationBeenRead { get; set; }

        /// <summary>
        /// Gets or sets the notification read by.
        /// </summary>
        /// <value>
        /// The notification read by.
        /// </value>
        public string NotificationReadBy { get; set; }

        /// <summary>
        /// Gets or sets the notification read at.
        /// </summary>
        /// <value>
        /// The notification read at.
        /// </value>
        public DateTime? NotificationReadAt { get; set; }

        /// <summary>
        /// Gets or sets the contract allocation number.
        /// </summary>
        /// <value>
        /// The contract allocation number.
        /// </value>
        public string ContractAllocationNumber { get; set; }

        /// <summary>
        /// Gets or sets the created at.
        /// </summary>
        /// <value>
        /// The created at.
        /// </value>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the last updated at.
        /// </summary>
        /// <value>
        /// The last updated at.
        /// </value>
        public DateTime LastUpdatedAt { get; set; }
    }
}