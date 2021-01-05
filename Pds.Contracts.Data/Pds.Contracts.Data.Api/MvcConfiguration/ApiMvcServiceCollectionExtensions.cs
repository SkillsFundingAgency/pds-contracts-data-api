using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.DependencyInjection;

namespace Pds.Contracts.Data.Api.MvcConfiguration
{
    /// <summary>
    /// Extension methods for setting up API MVC services in an <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ApiMvcServiceCollectionExtensions
    {
        /// <summary>
        /// Adds services for API controllers to the specified <see cref="IServiceCollection"/>,
        /// using custom conventions for routing.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        public static void AddApiControllers(this IServiceCollection services)
        {
            services.AddControllers(options =>
                options.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer())));
        }
    }
}