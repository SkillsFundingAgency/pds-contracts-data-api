using Microsoft.EntityFrameworkCore;
using Pds.Contracts.Data.Repository.Interfaces;
using System.Threading.Tasks;

namespace Pds.Contracts.Data.Repository.Implementations
{
    /// <summary>
    /// Single unit of work for repositories <see cref="Pds.Contracts.Data.Repository.Interfaces.IRepository{T}"/>.
    /// </summary>
    /// <seealso cref="Pds.Contracts.Data.Repository.Interfaces.IUnitOfWork" />
    public class SingleUnitOfWorkForRepositories : IUnitOfWork
    {
        /// <summary>
        /// The database context.
        /// </summary>
        private readonly DbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="SingleUnitOfWorkForRepositories"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public SingleUnitOfWorkForRepositories(DbContext dbContext) => _dbContext = dbContext;

        /// <inheritdoc/>
        public async Task CommitAsync() => await _dbContext.SaveChangesAsync();
    }
}