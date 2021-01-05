using Pds.Contracts.Data.Repository.Context;
using Pds.Contracts.Data.Repository.Interfaces;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Pds.Contracts.Data.Repository.Implementations
{
    /// <summary>
    /// Implementation of generic repository pattern.
    /// </summary>
    /// <typeparam name="T">Model.</typeparam>
    /// <seealso cref="Pds.Contracts.Data.Repository.Interfaces.IRepository{T}" />
    public class Repository<T> : IRepository<T>, IUnitOfWork
        where T : class
    {
        private readonly PdsContext pdsContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="Repository{T}"/> class.
        /// </summary>
        /// <param name="pdsContext">The PDS context.</param>
        public Repository(PdsContext pdsContext)
        {
            this.pdsContext = pdsContext;
        }

        /// <inheritdoc/>
        public async Task CommitAsync() => await pdsContext.SaveChangesAsync();

        /// <inheritdoc/>
        public async Task AddAsync(T entity) => await pdsContext.AddAsync(entity);

        /// <inheritdoc/>
        public IQueryable<T> GetAll() => pdsContext.Set<T>().AsQueryable();

        /// <inheritdoc/>
        public async Task<T> GetByIdAsync(int id) => await pdsContext.FindAsync<T>(id).ConfigureAwait(false);

        /// <inheritdoc/>
        public T GetByPredicate(Expression<Func<T, bool>> where) => pdsContext.Set<T>().SingleOrDefault(where);

        /// <inheritdoc/>
        public IQueryable<T> GetMany(Expression<Func<T, bool>> where) => pdsContext.Set<T>().Where(where);

        /// <inheritdoc/>
        public void Update(T entity) => _ = pdsContext.Set<T>().Update(entity);
    }
}