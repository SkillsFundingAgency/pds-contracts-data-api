using Pds.Contracts.Data.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Pds.Contracts.Data.Repository.Extensions
{
    /// <summary>
    /// Queryable order by extension.
    /// </summary>
    public static class OrderByExtension
    {
        /// <summary>
        /// Order by dynamic.
        /// </summary>
        /// <typeparam name="T">Entity model type T.</typeparam>
        /// <param name="query">Queryable of type T.</param>
        /// <param name="orderByMember">Member to be ordered by.</param>
        /// <param name="direction">Required sort order.</param>
        /// <returns>Returns a dynamically sorted queryable of type T.</returns>
        public static IQueryable<T> OrderByDynamic<T>(this IQueryable<T> query, string orderByMember, SortDirection direction)
        {
            var queryElementTypeParam = Expression.Parameter(typeof(T));

            var memberAccess = Expression.PropertyOrField(queryElementTypeParam, orderByMember);

            var keySelector = Expression.Lambda(memberAccess, queryElementTypeParam);

            var orderBy = Expression.Call(
                typeof(Queryable),
                direction == SortDirection.Asc ? "OrderBy" : "OrderByDescending",
                new Type[] { typeof(T), memberAccess.Type },
                query.Expression,
                Expression.Quote(keySelector));

            return query.Provider.CreateQuery<T>(orderBy);
        }
    }
}