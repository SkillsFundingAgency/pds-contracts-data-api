using Microsoft.EntityFrameworkCore;
using Pds.Contracts.Data.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.Contracts.Data.Repository.Implementations
{
    /// <summary>
    /// Paged list model of type T.
    /// </summary>
    /// <typeparam name="T">Generic type.</typeparam>
    public class PagedList<T> : IPagedList<T>
    {
        /// <inheritdoc/>
        public IEnumerable<T> Items { get; set; }

        /// <inheritdoc/>
        public int CurrentPage { get; set; }

        /// <inheritdoc/>
        public int TotalPages { get; set; }

        /// <inheritdoc/>
        public int PageSize { get; set; }

        /// <inheritdoc/>
        public int TotalCount { get; set; }

        /// <inheritdoc/>
        public bool HasPreviousPage => CurrentPage > 1;

        /// <inheritdoc/>
        public bool HasNextPage => CurrentPage < TotalPages;

        /// <inheritdoc/>
        public int? NextPageNumber => HasNextPage ? CurrentPage + 1 : (int?)null;

        /// <inheritdoc/>
        public int? PreviousPageNumber => HasPreviousPage ? CurrentPage - 1 : (int?)null;

        /// <summary>
        /// Initializes a new instance of the <see cref="PagedList{T}"/> class.
        /// </summary>
        /// <param name="items">List of items to create page list from.</param>
        /// <param name="count">The total number of items.</param>
        /// <param name="pageNumber">The current page number.</param>
        /// <param name="pageSize">Number of items per page.</param>
        public PagedList(List<T> items, int count, int pageNumber, int pageSize)
        {
            TotalCount = count;
            PageSize = pageSize;
            CurrentPage = pageNumber;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            Items = items;
        }
    }
}