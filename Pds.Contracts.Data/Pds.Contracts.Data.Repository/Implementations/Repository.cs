using Microsoft.EntityFrameworkCore;
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
    public class Repository<T> : IRepository<T>
        where T : class
    {
        private readonly DbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="Repository{T}"/> class.
        /// </summary>
        /// <param name="dbContext">The PDS context.</param>
        public Repository(DbContext dbContext) => _dbContext = dbContext;

        /// <inheritdoc/>
        public async Task AddAsync(T entity) => await _dbContext.AddAsync(entity);

        /// <inheritdoc/>
        public IQueryable<T> GetAll() => _dbContext.Set<T>();

        /// <inheritdoc/>
        public async Task<T> GetByIdAsync(int id) => await _dbContext.FindAsync<T>(id).ConfigureAwait(false);

        /// <inheritdoc/>
        public async Task<T> GetByPredicateAsync(Expression<Func<T, bool>> where) => _dbContext.Set<T>() is null ? null : await _dbContext.Set<T>().SingleOrDefaultAsync(where);

        /// <inheritdoc/>
        public IQueryable<T> GetMany(Expression<Func<T, bool>> where) => _dbContext.Set<T>()?.Where(where);

        /// <inheritdoc/>
        public void Update(T entity) => _ = _dbContext.Set<T>().Update(entity);
    }
}