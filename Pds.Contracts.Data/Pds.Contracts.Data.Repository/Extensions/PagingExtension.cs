using Microsoft.EntityFrameworkCore;
using Pds.Contracts.Data.Repository.Implementations;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.Contracts.Data.Repository.Extensions
{
    /// <summary>
    /// Paging extension.
    /// </summary>
    public static class PagingExtension
    {
        /// <summary>
        /// Create a paged list.
        /// </summary>
        /// <typeparam name="T">Entity model type T.</typeparam>
        /// <param name="source">Queryable object of type T.</param>
        /// <param name="pageNumber">Required page number.</param>
        /// <param name="pageSize">Required page size.</param>
        /// <returns>Returns a paged list object of type T.</returns>
        public static async Task<PagedList<T>> ToPagedList<T>(this IQueryable<T> source, int pageNumber, int pageSize)
        {
            var count = await source.CountAsync();
            var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PagedList<T>(items, count, pageNumber, pageSize);
        }
    }
}