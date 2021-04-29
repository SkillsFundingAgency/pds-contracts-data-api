using Pds.Contracts.Data.Services.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DataModels = Pds.Contracts.Data.Repository.DataModels;

namespace Pds.Contracts.Data.Services.Interfaces
{
    /// <summary>
    /// Service to assist in the retrieval of contract documents.
    /// </summary>
    public interface IContractDocumentService
    {
        /// <summary>
        /// Update or Insert contract document.
        /// </summary>
        /// <param name="contract">The contract to update.</param>
        /// <param name="request">The request that contains the filename.</param>
        /// <returns>Returns void.</returns>
        Task UpsertOriginalContractXmlAsync(DataModels.Contract contract, ContractRequest request);
    }
}
