using System.ComponentModel.DataAnnotations;

namespace Pds.Contracts.Data.Common.Enums
{
    /// <summary>
    /// Defines the amendment type of the contract.
    /// </summary>
    public enum ContractAmendmentType
    {
        /// <summary>
        /// Defines that this is version 1 of a Contract and has not been changed.
        /// </summary>
        [Display(Name = "")]
        None = 0,

        /// <summary>
        /// Represents a contract change where the contract was approved automatically.
        /// </summary>
        [Display(Name = "Notification")]
        Notfication = 1,

        /// <summary>
        /// Represents a contract change where the contract was changed but still needs approval from the provider it belongs too.
        /// </summary>
        [Display(Name = "Variation")]
        Variation = 2
    }
}
