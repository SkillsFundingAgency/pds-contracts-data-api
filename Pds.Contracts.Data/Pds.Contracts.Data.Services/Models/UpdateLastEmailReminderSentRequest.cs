using Pds.Contracts.Data.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pds.Contracts.Data.Services.Models
{
    /// <inheritdoc/>
    public class UpdateLastEmailReminderSentRequest : IUpdateLastEmailReminderSentRequest
    {
        /// <inheritdoc/>
        public string ContractNumber { get; set; }

        /// <inheritdoc/>
        public int ContractVersion { get; set; }
    }
}
