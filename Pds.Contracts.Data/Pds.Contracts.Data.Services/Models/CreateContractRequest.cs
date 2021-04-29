using Pds.Contracts.Data.Common.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Pds.Contracts.Data.Services.Models
{
    /// <summary>
    /// Request object that outlines the data required to create a contract.
    /// </summary>
    public class CreateContractRequest
    {
        /// <summary>
        /// Gets or sets the UKPRN assocaited with the contract.
        /// </summary>
        [Required]
        [RegularExpression("^[0-9]{8}$", ErrorMessage = "UKPRN should consist of 8 digits.")]
        public int UKPRN { get; set; }

        /// <summary>
        /// Gets or sets the title for the contract.
        /// </summary>
        [Required]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the unique contract number.
        /// </summary>
        [Required]
        [StringLength(20, ErrorMessage = "Contract number must be a value of no more than 20 characters.")]
        public string ContractNumber { get; set; }

        /// <summary>
        /// Gets or sets the contract version.
        /// </summary>
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please enter a value greater than zero.")]
        public int ContractVersion { get; set; }

        /// <summary>
        /// Gets or sets the value of the contract.
        /// </summary>
        [Required]
        [DataType(DataType.Currency)]
        [RegularExpression(@"[0-9]*\.?[0-9]+", ErrorMessage = "Please enter a numerical value")]
        public decimal Value { get; set; }

        /// <summary>
        /// Gets or sets the funding type associated with the contract.
        /// </summary>
        [Required]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ContractFundingType FundingType { get; set; }

        /// <summary>
        /// Gets or sets the year of the contract.
        /// </summary>
        [Required]
        [StringLength(12, ErrorMessage = "The year must be a value of no more than 12 characters.")]
        public string Year { get; set; }

        /// <summary>
        /// Gets or sets the contract type.
        /// </summary>
        [Required]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ContractType Type { get; set; }

        /// <summary>
        /// Gets or sets the number of the associated parent parent contract.
        /// </summary>
        [StringLength(20, ErrorMessage = "Contract number must be a value of no more than 20 characters.")]
        public string ParentContractNumber { get; set; }

        /// <summary>
        /// Gets or sets the start date of the contract.
        /// </summary>
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString ="{0:yyyy-MM-dd}")]
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date of the contract.
        /// </summary>
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Gets or sets the end date of the contract.
        /// </summary>
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime? SignedOn { get; set; }

        /// <summary>
        /// Gets or sets the amendment type.
        /// </summary>
        [Required]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ContractAmendmentType AmendmentType { get; set; }

        /// <summary>
        ///  Gets or sets the contract allocation number.
        /// </summary>
        public string ContractAllocationNumber { get; set; }

        /// <summary>
        /// Gets or sets the first census date id.
        /// </summary>
        public int? FirstCensusDateId { get; set; }

        /// <summary>
        /// Gets or sets the second census date id.
        /// </summary>
        public int? SecondCensusDateId { get; set; }

        /// <summary>
        /// Gets or sets the created by.
        /// </summary>
        [Required]
        public string CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the contract PDF contents.
        /// </summary>
        [Required]
        public CreateContractRequestDocument ContractContent { get; set; }

        /// <summary>
        /// Gets or sets the page count.
        /// </summary>
        [Required]
        public int PageCount { get; set; }

        /// <summary>
        /// Gets or sets the URI of the orginal XML file for the contract from blob storage.
        /// </summary>
        [Required]
        public string ContractData { get; set; }

        /// <summary>
        /// Gets or sets a collection of contract funding stream period codes.
        /// </summary>
        [Required]
        public CreateContractCode[] ContractFundingStreamPeriodCodes { get; set; }
    }
}
