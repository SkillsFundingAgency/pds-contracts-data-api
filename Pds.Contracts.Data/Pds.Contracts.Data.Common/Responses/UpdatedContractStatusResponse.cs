using System;
using System.Collections.Generic;
using System.Text;

namespace Pds.Contracts.Data.Common.Responses
{
    /// <summary>
    /// Updated Contract Status Response class.
    /// </summary>
    public class UpdatedContractStatusResponse
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the ukprn.
        /// </summary>
        /// <value>The ukprn.</value>
        public int Ukprn { get; set; }

        /// <summary>
        /// Gets or sets the contract number.
        /// </summary>
        /// <value>The contract number.</value>
        public string ContractNumber { get; set; }

        /// <summary>
        /// Gets or sets the contract version.
        /// </summary>
        /// <value>The contract version.</value>
        public int ContractVersion { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>The status.</value>
        public int Status { get; set; }

        /// <summary>
        /// Gets or sets the new status.
        /// </summary>
        /// <value>The status.</value>
        public int NewStatus { get; set; }
    }
}