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
    }
}