using System.Collections.Generic;

namespace Pds.Contracts.Data.Repository.Interfaces
{
    /// <summary>
    /// Paged list interface.
    /// </summary>
    /// <typeparam name="T">Type.</typeparam>
    public interface IPagedList<T>
    {
        /// <summary>
        /// Gets or sets enumerable items.
        /// </summary>
        public IEnumerable<T> Items { get; set; }

        /// <summary>
        /// Gets or sets current page.
        /// </summary>
        int CurrentPage { get; set; }

        /// <summary>
        /// Gets a value indicating whether a next page is available.
        /// </summary>
        bool HasNextPage { get; }

        /// <summary>
        /// Gets a value indicating whether a previous page is available.
        /// </summary>
        bool HasPreviousPage { get; }

        /// <summary>
        /// Gets a value indicating whether is a next page number in sequence.
        /// </summary>
        int? NextPageNumber { get; }

        /// <summary>
        /// Gets or sets required page size.
        /// </summary>
        int PageSize { get; set; }

        /// <summary>
        /// Gets a value indicating whether is a previous page number in sequence.
        /// </summary>
        int? PreviousPageNumber { get; }

        /// <summary>
        /// Gets or sets total row count.
        /// </summary>
        int TotalCount { get; set; }

        /// <summary>
        /// Gets or sets total number of pages.
        /// </summary>
        int TotalPages { get; set; }
    }
}