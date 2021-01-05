#nullable disable

namespace Pds.Contracts.Data.Repository.DataModels
{
    /// <summary>
    /// ContractData data model, auto generated based on reverse engirneering ContractData table structure from PDS database.
    /// Should not be changed manually.
    /// </summary>
    public partial class ContractData
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the original contract XML.
        /// </summary>
        /// <value>
        /// The original contract XML.
        /// </value>
        public string OriginalContractXml { get; set; }

        /// <summary>
        /// Gets or sets the identifier navigation.
        /// </summary>
        /// <value>
        /// The identifier navigation.
        /// </value>
        public virtual Contract IdNavigation { get; set; }
    }
}