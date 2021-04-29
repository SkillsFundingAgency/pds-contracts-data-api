using System;
using System.Collections.Generic;
using System.Text;

namespace Pds.Contracts.Data.Services.ConfigurationOptions
{
    /// <summary>
    /// Service Bus Options.
    /// </summary>
    public class ServiceBusOptions
    {
        /// <summary>
        /// Gets or sets the service bus connection String.
        /// </summary>
        public string ServiceBusConnectionString { get; set; }

        /// <summary>
        ///  Gets or sets the service bus topic Name.
        /// </summary>
        public string TopicName { get; set; }

        /// <summary>
        /// Gets or sets the retry count.
        /// </summary>
        /// <value>The retry count.</value>
        public int RetryCount { get; set; } = 3;

        /// <summary>
        /// Gets or sets the minimum backoff TimeSpan in seconds.
        /// </summary>
        /// <value>The retry minimum backoff.</value>
        public double MinimumBackoff { get; set; } = 2;

        /// <summary>
        /// Gets or sets the maximum backoff TimeSpan in seconds.
        /// </summary>
        /// <value>The retry maximum backoff.</value>
        public double MaximumBackoff { get; set; } = 10;
    }
}
