﻿using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Text.Json.Serialization;

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
        /// <returns>Returns an amended IServiceCollection.</returns>
        public static IServiceCollection AddApiControllers(this IServiceCollection services)
        {
            services.AddControllers(options =>
                options.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer())))
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

            return services;
        }
    }
}