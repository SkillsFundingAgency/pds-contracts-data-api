#nullable disable

namespace Pds.Contracts.Data.Repository.DataModels
{
    /// <summary>
    /// ContractContent data model, auto generated based on reverse engirneering ContactContent table structure from PDS database.
    /// Should not be changed manually.
    /// </summary>
    public partial class ContractContent
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the content.
        /// </summary>
        /// <value>
        /// The content.
        /// </value>
        public byte[] Content { get; set; }

        /// <summary>
        /// Gets or sets the size.
        /// </summary>
        /// <value>
        /// The size.
        /// </value>
        public long Size { get; set; }

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        /// <value>
        /// The name of the file.
        /// </value>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the identifier navigation.
        /// </summary>
        /// <value>
        /// The identifier navigation.
        /// </value>
        public virtual Contract IdNavigation { get; set; }
    }
}