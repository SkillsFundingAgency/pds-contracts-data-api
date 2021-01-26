using System;
using System.ComponentModel.DataAnnotations;

namespace Pds.Contracts.Data.Services.Models.Enums
{
    /// <summary>
    /// A list of sort options to apply to a query.
    /// </summary>
    public enum ContractSortOptions
    {
        /// <summary>
        /// The unique identifier for each client.
        /// </summary>
        [Display(Name = "UKPRN", Description = "UKPRN")]
        UKPRN = 1,

        /// <summary>
        /// The title of the contracts.
        /// </summary>
        [Display(Name = "Title", Description = "Title of the contract")]
        Title = 2,

        /// <summary>
        /// The unique reference for each contract.
        /// </summary>
        [Display(Name = "Contract Number", Description = "The unique reference for each contract.")]
        ContractNumber = 3,

        /// <summary>
        /// The unique reference for the parent contract.
        /// </summary>
        [Display(Name = "Parent Contract Number", Description = "The unique reference for the parent contract.")]
        ParentContractNumber = 4,

        /// <summary>
        /// The version number of the current contract.
        /// </summary>
        [Display(Name = "Contract Version", Description = "The version number of the current contract.")]
        ContractVersion = 5,

        /// <summary>
        /// The value of the contract.
        /// </summary>
        [Display(Name = "Contract Value", Description = "The value of the contract.")]
        Value = 6,

        /// <summary>
        /// The statuses of the contract.
        /// </summary>
        [Display(Name = "Contract Status", Description = "The statuses of the contract.")]
        Status = 7,

        /// <summary>
        /// The funding type for the contract.
        /// </summary>
        [Display(Name = "Funding Type", Description = "The funding type for the contract.")]
        FundingType = 8,

        /// <summary>
        /// The start date of the contract.
        /// </summary>
        [Display(Name = "Contract Start Date", Description = "The start date of the contract.")]
        StartDate = 9,

        /// <summary>
        /// The end date of the contract.
        /// </summary>
        [Display(Name = "Contract End Date", Description = "The end date of the contract.")]
        EndDate = 10,

        /// <summary>
        /// The amendment types for the contract.
        /// </summary>
        [Display(Name = "Amendment Type", Description = "The amendment types for the contract.")]
        AmendmentType = 11,

        /// <summary>
        /// The date and time the last reminder was sent at.
        /// </summary>
        [Display(Name = "Last Email Reminder Sent", Description = "The date and time the last reminder was sent at.")]
        LastEmailReminderSent = 12,

        /// <summary>
        /// The contract allocation number.
        /// </summary>
        [Display(Name = "Contract Allocation Number", Description = "The contract allocation number.")]
        ContractAllocationNumber = 13,

        /// <summary>
        /// The datetime at which the record was created at.
        /// </summary>
        [Display(Name = "Created At", Description = "The datetime at which the record was created at.")]
        CreatedAt = 14,

        /// <summary>
        /// The datetime at which the record was last updated at.
        /// </summary>
        [Display(Name = "Last Updated At", Description = "The datetime at which the record was last updated at.")]
        LastUpdatedAt = 15
    }
}