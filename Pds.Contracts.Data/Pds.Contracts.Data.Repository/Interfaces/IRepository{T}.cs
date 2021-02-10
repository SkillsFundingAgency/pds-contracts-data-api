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
        /// <param name="old">The old entity.</param>
        /// <param name="current">The current entity.</param>
        void Patch(T old, T current);
    }
}