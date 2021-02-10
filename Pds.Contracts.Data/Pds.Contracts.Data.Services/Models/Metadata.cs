namespace Pds.Contracts.Data.Services.Models
{
    /// <summary>
    /// Metadata.
    /// </summary>
    public class Metadata
    {
        /// <summary>
        /// Gets or sets totalCount.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Gets or sets PageSize.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Gets or sets CurrentPage.
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// Gets or sets TotalPages.
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets HasNextPage.
        /// </summary>
        public bool HasNextPage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets HasPreviousPage.
        /// </summary>
        public bool HasPreviousPage { get; set; }

        /// <summary>
        /// Gets or sets NextPageUrl.
        /// </summary>
        public string NextPageUrl { get; set; }

        /// <summary>
        /// Gets or sets PreviousPageUrl.
        /// </summary>
        public string PreviousPageUrl { get; set; }
    }
}