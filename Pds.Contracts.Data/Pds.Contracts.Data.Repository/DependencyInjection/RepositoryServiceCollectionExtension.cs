using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pds.Contracts.Data.Repository.Implementations;
using Pds.Contracts.Data.Repository.Interfaces;

namespace Pds.Contracts.Data.Repository.DependencyInjection
{
    /// <summary>
    /// IServiceCollection extensions for adding repository DI to container.
    /// </summary>
    public static class RepositoryServiceCollectionExtension
    {
        /// <summary>
        /// Adds services for the current feature to the specified <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add the feature's services to.</param>
        /// <param name="configuration">The configuration.</param>
        /// <returns>
        /// A reference to this instance after the operation has completed.
        /// </returns>
        public static IServiceCollection AddRepositoriesServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            return services;
        }
    }
}