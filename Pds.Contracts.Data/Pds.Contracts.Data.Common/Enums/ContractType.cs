using System.ComponentModel.DataAnnotations;

namespace Pds.Contracts.Data.Common.Enums
{
    /// <summary>
    /// The types of contract that the system supports.
    /// </summary>
    public enum ContractType
    {
        /// <summary>
        /// The conditions of funding grant.
        /// </summary>
        [Display(Name = "Condition of funding (Grant)")]
        ConditionsOfFundingGrant = 0,

        /// <summary>
        /// A contract type of an employer grant.
        /// </summary>
        [Display(Name = "Condition of funding (Grant) employer")]
        ConditionsOfFundingGrantEmployer = 1,

        /// <summary>
        /// A contract type of a large employer.
        /// </summary>
        [Display(Name = "Condition of funding (Large Employers)")]
        ConditionsOfFundingLargeEmployers = 2,

        /// <summary>
        /// A contract type of a large employer for outcome pilot.
        /// </summary>
        [Display(Name = "Condition of funding (Large employers-Outcome pilot)")]
        ConditionsOfFundingLargeEmployersOutcomePilot = 3,

        /// <summary>
        /// A contract type for services.
        /// </summary>
        [Display(Name = "Contract For Services")]
        ContractForServices = 4,

        /// <summary>
        /// A contract type of a financial memorandum.
        /// </summary>
        [Display(Name = "Financial Memorandum")]
        FinancialMemorandum = 5,

        /// <summary>
        /// A contract type of a 56F financial memorandum.
        /// </summary>
        [Display(Name = "Financial Memorandum (S6F)")]
        FinancialMemorandum56F = 6,

        /// <summary>
        /// A contract type of a EOI twenty four plus loan.
        /// </summary>
        [Display(Name = "24+ Advanced learning loan EOI")]
        TwentyFourPlusAdvancedLearningLoanEoi = 7,

        /// <summary>
        /// A twenty four plus loan contract type for facility conditions.
        /// </summary>
        [Display(Name = "24+ Advanced learning loan facility conditions")]
        TwentyFourPlusAdvancedLearningLoanFacilityConditions = 8
    }
}
