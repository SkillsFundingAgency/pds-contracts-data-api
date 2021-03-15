using System;

namespace Pds.Contracts.Data.Services.Interfaces
{
    /// <summary>
    /// IUriService.
    /// </summary>
    public interface IUriService
    {
        /// <summary>
        /// Get Uri.
        /// </summary>
        /// <param name="actionUrl">action Url.</param>
        /// <returns>Uri.</returns>
        Uri GetUri(string actionUrl);
    }
}