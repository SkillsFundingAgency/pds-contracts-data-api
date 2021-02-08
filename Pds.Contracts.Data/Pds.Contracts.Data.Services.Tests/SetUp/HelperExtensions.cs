using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Pds.Contracts.Data.Repository.Context;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Pds.Contracts.Data.Services.Tests.SetUp
{
    public static class HelperExtensions
    {
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