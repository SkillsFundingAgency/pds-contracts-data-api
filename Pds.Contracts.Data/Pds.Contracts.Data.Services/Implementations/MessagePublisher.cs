using Microsoft.Azure.ServiceBus;
using Pds.Contracts.Data.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Pds.Contracts.Data.Services.Implementations
{
    /// <summary>
    /// Message Publisher.
    /// </summary>
    public class MessagePublisher : IMessagePublisher
    {
        private readonly ITopicClient topicClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagePublisher"/> class.
        /// </summary>
        /// <param name="topicClient">Azure service bus topic client.</param>
        public MessagePublisher(ITopicClient topicClient)
        {
            this.topicClient = topicClient;
        }

        /// <inheritdoc/>
        public async Task PublisherAsync<T>(T request, IDictionary<string, string> properties)
        {
            var message = new Message
            {
                MessageId = Guid.NewGuid().ToString(),
                Body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(request)),
            };

            if (properties?.Count > 0)
            {
                foreach (var item in properties.Keys)
                {
                    message.UserProperties.Add(item, properties[item]);
                }
            }

            await topicClient.SendAsync(message);
        }
    }
}
