using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Pds.Contracts.Data.Services.Interfaces
{
    /// <summary>
    /// Message publisher.
    /// </summary>
    public interface IMessagePublisher
    {
        /// <summary>
        /// Send Message to service bus.
        /// </summary>
        /// <typeparam name="T">Message Type.</typeparam>
        /// <param name="request">Message.</param>
        /// <param name="properties">Key Value pairs for the message User Properties.</param>
        /// <returns>Task.</returns>
        Task PublisherAsync<T>(T request, IDictionary<string, string> properties);
    }
}
