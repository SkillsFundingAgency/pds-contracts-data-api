using Microsoft.EntityFrameworkCore.Query;
using Pds.Contracts.Data.Repository.DataModels;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Pds.Contracts.Data.Repository.Interfaces
{
    /// <summary>
    /// Repository pattern interface.
    /// </summary>
    /// <typeparam name="T">Model.</typeparam>
    public interface IRepository<T>
        where T : class
    {
        /// <summary>
        /// Gets the <typeparamref name="T"/> by identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>Instance of <typeparamref name="T"/>.</returns>
        Task<T> GetByIdAsync(int id);

        /// <summary>
        /// Gets <typeparamref name="T"/> the by predicate.
        /// </summary>
        /// <param name="where">The where predicate.</param>
        /// <returns>Instance of <typeparamref name="T"/>.</returns>
        Task<T> GetByPredicateAsync(Expression<Func<T, bool>> where);

        /// <summary>
        /// Gets <typeparamref name="T"/> the by predicate, including child types by navigation property.
        /// </summary>
        /// <typeparam name="TProperty">Child navigation property type.</typeparam>
        /// <param name="where">Predicate used to select a single <typeparamref name="T"/>.</param>
        /// <param name="navigationPropertyPath">Navigation property to child type <typeparamref name="TProperty"/>.</param>
        /// <returns>Instance of <typeparamref name="T"/>.</returns>
        Task<T> GetByPredicateWithIncludeAsync<TProperty>(Expression<Func<T, bool>> where, Expression<Func<T, TProperty>> navigationPropertyPath);

        /// <summary>
        /// Gets all <typeparamref name="T"/>.
        /// </summary>
        /// <returns>A <see cref="IQueryable{T}"/> of all <typeparamref name="T"/>.</returns>
        IQueryable<T> GetAll();

        /// <summary>
        /// Gets many items of instance of <typeparamref name="T"/>.
        /// </summary>
        /// <param name="where">The where predicate.</param>
        /// <returns>A <see cref="IQueryable{T}"/> of all <typeparamref name="T"/>.</returns>
        IQueryable<T> GetMany(Expression<Func<T, bool>> where);

        /// <summary>
        /// Adds the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>Async task.</returns>
        Task AddAsync(T entity);

        /// <summary>
        /// Updates the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        void Update(T entity);

        /// <summary>
        /// Updates the specified entity.
        /// </summary>
        /// <param name="id">Id of the entity that should be updated.</param>
        /// <param name="current">The current entity.</param>
        /// <returns>Async completion task.</returns>
        Task PatchAsync(int id, T current);

        /// <summary>
        /// Gets the first or default entity based on a predicate and include delegate.
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <param name="includes">A function to include navigation properties.</param>
        /// <returns>An <see cref="{T}"/> that contains elements that satisfy the condition specified by <paramref name="predicate"/>.</returns>
        Task<T> GetFirstOrDefault(
                                    Expression<Func<T, bool>> predicate,
                                    Func<IQueryable<T>, IIncludableQueryable<T, object>> includes);
    }
}