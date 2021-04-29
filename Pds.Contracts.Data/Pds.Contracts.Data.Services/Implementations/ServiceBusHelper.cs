using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Pds.Contracts.Data.Services.ConfigurationOptions;
using System;

namespace Pds.Contracts.Data.Services.Implementations
{
    /// <summary>
    /// Service Bus Helper class.
    /// </summary>
    public class ServiceBusHelper
    {
        /// <summary>
        /// Get Topic Client.
        /// </summary>
        /// <param name="config">IConfiguration.</param>
        /// <returns>TopicClient.</returns>
        public static TopicClient GetTopicClient(IConfiguration config)
        {
            var notificationTopicSBOptions = new ServiceBusOptions();
            config.GetSection("NotificationTopicSBOptions").Bind(notificationTopicSBOptions);

            var topicClient = new TopicClient(notificationTopicSBOptions.ServiceBusConnectionString, notificationTopicSBOptions.TopicName, GetRetryPolicy(notificationTopicSBOptions));
            return topicClient;
        }

        private static RetryExponential GetRetryPolicy(ServiceBusOptions notificationTopicSBOptions)
        {
            return new RetryExponential(TimeSpan.FromSeconds(notificationTopicSBOptions.MinimumBackoff), TimeSpan.FromSeconds(notificationTopicSBOptions.MaximumBackoff), notificationTopicSBOptions.RetryCount);
        }
    }
}
