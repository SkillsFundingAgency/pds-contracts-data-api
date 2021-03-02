using System.Threading.Tasks;

namespace Pds.Contracts.Data.Repository.Interfaces
{
    /// <summary>
    /// Unit of work helps to manage transaction across more than one data setss.
    /// </summary>
    public interface IUnitOfWork
    {
        /// <summary>
        /// Commits this instance asynchronously.
        /// </summary>
        /// <returns>Async task.</returns>
        Task CommitAsync();

        /// <summary>
        /// Is this entity instance tracked.
        /// </summary>
        /// <typeparam name="T">Type of entity.</typeparam>
        /// <param name="entity">Instance that should be checked.</param>
        /// <returns>True if this entity is tracked.</returns>
        bool IsTracked<T>(T entity)
            where T : class;
    }
}