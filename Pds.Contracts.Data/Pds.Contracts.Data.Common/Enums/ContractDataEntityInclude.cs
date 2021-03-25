using System;

namespace Pds.Contracts.Data.Common.Enums
{
    /// <summary>
    /// Defines the bitwise elements for contract data entity includes.
    /// </summary>
    [Flags]
    public enum ContractDataEntityInclude
    {
        /// <summary>
        /// Only include datas.
        /// </summary>
        Datas = 1,

        /// <summary>
        /// Only include content.
        /// </summary>
        Content = 2
    }
}
