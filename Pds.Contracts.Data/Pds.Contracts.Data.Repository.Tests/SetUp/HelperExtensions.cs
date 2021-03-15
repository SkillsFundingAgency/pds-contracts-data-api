using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Pds.Contracts.Data.Repository.Context;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Pds.Contracts.Data.Repository.Tests.SetUp
{
    public static class HelperExtensions
    {
        /// <summary>
        /// Gets mock of database set, with configuration to use <see cref="ICollection{T}"/> for LINQ.
        /// </summary>
        /// <typeparam name="T">Should be an class that can have instances.</typeparam>
        /// <param name="entities">The collection that should be used for DbSet.</param>
        /// <returns>Mock of DbSet.</returns>
        internal static Mock<DbSet<T>> GetMockDbSet<T>(this ICollection<T> entities)
            where T : class, new()
        {
            var mockSet = new Mock<DbSet<T>>(MockBehavior.Strict);
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(entities.AsQueryable().Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(entities.AsQueryable().Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(entities.AsQueryable().ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(entities.AsQueryable().GetEnumerator());

            return mockSet;
        }

        /// <summary>
        /// Provides a new instance of in memory PDS database context.
        /// </summary>
        /// <returns>An instance of <see cref="PdsContext"/> that uses in-memory database.</returns>
        internal static PdsContext GetInMemoryPdsDbContext()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            var options = new DbContextOptionsBuilder<PdsContext>()
                .UseInMemoryDatabase(databaseName: "InMemoryPdsDatabase")
                .UseInternalServiceProvider(serviceProvider)
                .Options;

            return new PdsContext(options);
        }
    }
}