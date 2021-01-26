using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Pds.Contracts.Data.Services.Models.Enums
{
    /// <summary>
    /// Enumeration to detemine the sort order.
    /// </summary>
    public enum SortDirection
    {
        /// <summary>
        /// Ascending sort order.
        /// </summary>
        [Display(Name = "Asc", Description = "Ascending")]
        Asc = 0,

        /// <summary>
        /// Descending sort order.
        /// </summary>
        [Display(Name = "Desc", Description = "Descending")]
        Desc = 1
    }
}