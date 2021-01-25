using Pds.Contracts.Data.Services.Models.Enums;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Pds.Contracts.Data.Services.Models
{
    /// <summary>
    /// A paged collection of contracts that need a reminder.
    /// </summary>
    public class ContractReminders
    {
        /// <summary>
        /// Gets or sets list of contracts.
        /// </summary>
        public IList<Contract> Contracts { get; set; }

        /// <summary>
        /// Gets or sets the page number for the current dataset.
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// Gets or sets the number of records in the current page.
        /// </summary>
        public int PageCount { get; set; }

        /// <summary>
        /// Gets or sets the total number of pages.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Gets or sets the sorting criteria used.
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ContractSortOptions SortedBy { get; set; }

        /// <summary>
        /// Gets or sets the sort direction.
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public SortDirection Order { get; set; }
    }
}
