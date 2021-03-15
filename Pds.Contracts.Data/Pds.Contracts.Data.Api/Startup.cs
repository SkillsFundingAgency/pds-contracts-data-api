using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Pds.Contracts.Data.Api.MvcConfiguration;
using Pds.Contracts.Data.Services.DependencyInjection;
using Pds.Core.ApiAuthentication;
using Pds.Core.Logging;
using Pds.Core.Telemetry.ApplicationInsights;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.IO;

namespace Pds.Contracts.Data.Api
{
    /// <summary>
    /// The startup class.
    /// </summary>
    public class Startup
    {
        private const string RequireElevatedRightsPolicyName = "RequireElevatedRights";
        private const string CurrentApiVersion = "v1.0.0";
        private static string _assemblyName;

        private readonly IWebHostEnvironment _environment;

        /// <summary>
        /// Gets the application configuration.
        /// </summary>
        public IConfiguration Configuration { get; }

        private string AssemblyName
        {
            get
            {
                if (string.IsNullOrEmpty(_assemblyName))
                {
                    _assemblyName = this.GetType().Assembly.GetName().Name;
                }

                return _assemblyName;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">The application configuration.</param>
        /// <param name="environment">Web host environment.</param>
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            _environment = environment;
        }

        /// <summary>
        /// Configures the services for the container.
        /// </summary>
        /// <param name="services">The service collection.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddApiControllers();

            services.AddFeatureServices(Configuration);
            services.AddHealthChecks().AddFeatureHealthChecks();
            services.AddPdsApplicationInsightsTelemetry(options => BuildAppInsightsConfiguration(options));
            services.AddLoggerAdapter();
            services.AddAzureADAuthentication(Configuration);

            if (_environment.IsDevelopment())
            {
                services.DisableAuthentication(AssemblyName);
            }

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(CurrentApiVersion, new OpenApiInfo { Title = AssemblyName, Version = CurrentApiVersion });

                // Set the comments path for the Swagger JSON and UI.
                var xmlPath = Path.Combine(AppContext.BaseDirectory, $"{AssemblyName}.xml");
                c.IncludeXmlComments(xmlPath);

                if (!_environment.IsDevelopment())
                {
                    AddOauth2BeararTokenAuthDefenition(c);
                }
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy(RequireElevatedRightsPolicyName, policy => policy.RequireRole("ContractsApiRole"));
            });
        }

        /// <summary>
        /// Configures the HTTP request pipeline.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <param name="env">The web hosting environment.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"/swagger/{CurrentApiVersion}/swagger.json", AssemblyName);
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health");
            });
        }

        private void BuildAppInsightsConfiguration(PdsApplicationInsightsConfiguration options)
        {
            Configuration.Bind("PdsApplicationInsights", options);
            options.Component = AssemblyName;
        }

        private void AddOauth2BeararTokenAuthDefenition(SwaggerGenOptions c)
        {
            var securityScheme = new OpenApiSecurityScheme
            {
                Name = "JWT Authentication",
                Description = "Enter JWT Bearer token",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer", // must be lower case
                BearerFormat = "JWT",
                Reference = new OpenApiReference
                {
                    Id = JwtBearerDefaults.AuthenticationScheme,
                    Type = ReferenceType.SecurityScheme
                }
            };

            c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { securityScheme, new string[] { } }
            });
        }
    }
}