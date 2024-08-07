﻿using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.ServiceBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Pds.Contracts.Data.Repository.Context;
using Pds.Contracts.Data.Repository.DependencyInjection;
using Pds.Contracts.Data.Services.DocumentServices;
using Pds.Contracts.Data.Services.Implementations;
using Pds.Contracts.Data.Services.Interfaces;
using Pds.Contracts.Data.Services.Models;
using Pds.Core.ApiClient.Interfaces;
using Pds.Core.ApiClient.Services;

namespace Pds.Contracts.Data.Services.DependencyInjection
{
    /// <summary>
    /// Extensions class for <see cref="IServiceCollection"/> for registering the feature's services.
    /// </summary>
    public static class FeatureServiceCollectionExtensions
    {
        /// <summary>
        /// Adds services for the current feature to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">
        /// The <see cref="IServiceCollection"/> to add the feature's services to.
        /// </param>
        /// <param name="configuration">The configuration.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection AddFeatureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<PdsContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("contracts"));
            });

            services.AddRepositoriesServices(configuration);
            services.AddAutoMapper(typeof(FeatureServiceCollectionExtensions).Assembly);
            services.AddScoped<IContractService, ContractService>();
            services.AddScoped<ISubcontractorDeclarationService, SubcontractorDeclarationService>();

            services.AddAsposeLicense();
            services.AddScoped<IDocumentManagementContractService, AsposeDocumentManagementContractService>();
            services.AddScoped<IDocumentManagementService, AsposeDocumentManagementService>();
            services.AddScoped<IContractValidationService, ContractValidationService>();

            services.AddTransient(typeof(IAuthenticationService<>), typeof(AuthenticationService<>));

            services.AddSingleton<IUriService>(provider =>
            {
                var accesor = provider.GetRequiredService<IHttpContextAccessor>();
                var request = accesor.HttpContext.Request;
                var absoluteUri = string.Concat(request.Scheme, "://", request.Host.ToUriComponent());
                return new UriService(absoluteUri);
            });

            services.AddSingleton(typeof(ISemaphoreOnEntity<>), typeof(SemaphoreOnEntity<>));

            services.AddSingleton<ITopicClient>(serviceProvider => ServiceBusHelper.GetTopicClient(configuration));
            services.AddSingleton<IMessagePublisher, MessagePublisher>();

            services.AddMediatR(typeof(FeatureServiceCollectionExtensions).Assembly);

            services.AddSingleton(s => Helpers.BlobHelper.GetBlobContainerClient(configuration));
            services.AddSingleton<IContractDocumentService, ContractDocumentService>();

            return services;
        }
    }
}