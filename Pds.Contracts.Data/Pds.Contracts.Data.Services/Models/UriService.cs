using Pds.Contracts.Data.Services.Interfaces;
using System;

namespace Pds.Contracts.Data.Services.Models
{
    /// <summary>
    /// UriService.
    /// </summary>
    public class UriService : IUriService
    {
        private readonly string _baseUri;

        /// <summary>
        /// Initializes a new instance of the <see cref="UriService"/> class.
        /// </summary>
        /// <param name="baseUri">base Uri.</param>
        public UriService(string baseUri)
        {
            _baseUri = baseUri;
        }

        /// <inheritdoc/>
        public Uri GetUri(string actionUrl)
        {
            return new Uri($"{_baseUri}{actionUrl}");
        }
    }
}