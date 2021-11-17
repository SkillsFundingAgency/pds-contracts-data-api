using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace Pds.Contracts.Data.Repository.DataModels
{
    /// <summary>
    /// Contract POCO.
    /// </summary>
    public partial class Contract
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Contract"/> class.
        /// </summary>
        public Contract()
        {
            ContractFundingStreamPeriodCodes = new HashSet<ContractFundingStreamPeriodCode>();
        }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
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
        public string ContractNumber { get; set; }

        /// <summary>
        /// Gets or sets the contract version.
        /// </summary>
        /// <value>
        /// The contract version.
        /// </value>
        public int ContractVersion { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public decimal Value { get; set; }

        /// <summary>
        /// Gets or sets the page count.
        /// </summary>
        /// <value>
        /// The page count.
        /// </value>
        public int PageCount { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public int Status { get; set; }

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
        [ConcurrencyCheck]
        public DateTime LastUpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the type of the funding.
        /// </summary>
        /// <value>
        /// The type of the funding.
        /// </value>
        public int FundingType { get; set; }

        /// <summary>
        /// Gets or sets the year.
        /// </summary>
        /// <value>
        /// The year.
        /// </value>
        public string Year { get; set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public int Type { get; set; }

        /// <summary>
        /// Gets or sets the display name of the signed by.
        /// </summary>
        /// <value>
        /// The display name of the signed by.
        /// </value>
        public string SignedByDisplayName { get; set; }

        /// <summary>
        /// Gets or sets the email reminder.
        /// </summary>
        /// <value>
        /// The email reminder.
        /// </value>
        public int? EmailReminder { get; set; }

        /// <summary>
        /// Gets or sets the parent contract number.
        /// </summary>
        /// <value>
        /// The parent contract number.
        /// </value>
        public string ParentContractNumber { get; set; }

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
        /// Gets or sets the created by.
        /// </summary>
        /// <value>
        /// The created by.
        /// </value>
        public string CreatedBy { get; set; }

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
        /// Gets or sets the content of the contract.
        /// </summary>
        /// <value>
        /// The content of the contract.
        /// </value>
        public virtual ContractContent ContractContent { get; set; }

        /// <summary>
        /// Gets or sets the contract data.
        /// </summary>
        /// <value>
        /// The contract data.
        /// </value>
        public virtual ContractData ContractData { get; set; }

        /// <summary>
        /// Gets or sets the contract funding stream period codes.
        /// </summary>
        /// <value>
        /// The contract funding stream period codes.
        /// </value>
        public virtual ICollection<ContractFundingStreamPeriodCode> ContractFundingStreamPeriodCodes { get; set; }
    }
}