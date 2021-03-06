﻿using Pds.Contracts.Data.Common.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pds.Contracts.Data.Services.Models
{
    /// <summary>
    /// Request withdraw object.
    /// </summary>
    public class UpdateContractWithdrawalRequest : ContractRequest
    {
        /// <summary>
        /// Gets or sets the contract status withdrawal type.
        /// </summary>
        /// <value>
        /// The contract withdrawal type, which can be WithdrawnByAgency [1] or WithdrawnByProvider [2].
        /// </value>
        [Required]
        [Range(1, 2, ErrorMessage = "Please enter a value of WithdrawnByAgency [1] or WithdrawnByProvider [2].")]
        public ContractStatus WithdrawalType { get; set; }
    }
}
