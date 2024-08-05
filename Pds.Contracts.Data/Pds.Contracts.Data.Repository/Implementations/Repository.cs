using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
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
        public async Task<T> GetByPredicateWithIncludeAsync<TProperty>(Expression<Func<T, bool>> where, Expression<Func<T, TProperty>> navigationPropertyPath) => await _dbContext.Set<T>().Include(navigationPropertyPath).SingleOrDefaultAsync(where);

        /// <inheritdoc/>
        public async Task<T> GetByPredicateAsync(Expression<Func<T, bool>> where) => _dbContext.Set<T>() is null ? null : await _dbContext.Set<T>().SingleOrDefaultAsync(where);

        /// <inheritdoc/>
        public IQueryable<T> GetMany(Expression<Func<T, bool>> where) => _dbContext.Set<T>()?.Where(where);

        /// <inheritdoc/>
        public void Update(T entity) => _ = _dbContext.Set<T>().Update(entity);

        /// <inheritdoc/>
        public async Task PatchAsync(int id, T current) => _dbContext.Entry<T>(await GetByIdAsync(id)).CurrentValues.SetValues(current);

        /// <inheritdoc/>
        public Task<T> GetFirstOrDefault(
                                            Expression<Func<T, bool>> predicate,
                                            Func<IQueryable<T>, IIncludableQueryable<T, object>> includes)
        {
            IQueryable<T> query = _dbContext.Set<T>();
            return Task.FromResult(includes(query).Where(predicate).FirstOrDefault());
        }
    }
}