#nullable disable

namespace Pds.Contracts.Data.Repository.DataModels
{
    /// <summary>
    /// ContractFundingStreamPeriodCode data model, auto generated based on reverse engirneering ContractFundingStreamPeriodCode table structure from PDS database.
    /// Should not be changed manually.
    /// </summary>
    public partial class ContractFundingStreamPeriodCode
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the code.
        /// </summary>
        /// <value>
        /// The code.
        /// </value>
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets the contract identifier.
        /// </summary>
        /// <value>
        /// The contract identifier.
        /// </value>
        public int? ContractId { get; set; }

        /// <summary>
        /// Gets or sets the contract.
        /// </summary>
        /// <value>
        /// The contract.
        /// </value>
        public virtual Contract Contract { get; set; }
    }
}