using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Pds.Contracts.Data.Api.Tests.Integration
{
    /// <summary>
    /// Integration Test Helper.
    /// </summary>
    public class IntegrationTest
    {
        private readonly HttpClient _testClient;

        internal HttpClient TestClient
        {
            get
            {
                return _testClient;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IntegrationTest"/> class.
        /// </summary>
        protected IntegrationTest()
        {
            var appFactory = new WebApplicationFactory<Startup>();
            _testClient = appFactory.CreateClient();
            _testClient.BaseAddress = new Uri("http://localhost:5001");
            _testClient.DefaultRequestHeaders.Accept.Clear();
            _testClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <summary>
        /// Create an async http client post.
        /// </summary>
        /// <param name="url">Host url.</param>
        /// <param name="request">Request.</param>
        /// <returns>Returns a task object of type http response message.</returns>
        protected async Task<HttpResponseMessage> CreatePostAsync(string url, StringContent request)
        {
            var response = await _testClient.PostAsync(url, request);
            return response;
        }
    }
}