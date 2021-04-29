using Pds.Contracts.Data.Common.Enums;
using System;

namespace Pds.Contracts.Data.Services.Interfaces
{
    /// <summary>
    /// IDocumentManagementContractService.
    /// </summary>
    public interface IDocumentManagementContractService : IDocumentManagementService
    {
        /// <summary>
        /// Adds a page to the pdf detailing that the contract was digitally signed.
        /// </summary>
        /// <param name="pdf">The PDF to add the page to.</param>
        /// <param name="contractRefernce">The contract reference.</param>
        /// <param name="signer">Who signed the contract.</param>
        /// <param name="signedOn">When the contract was signed.</param>
        /// <param name="manuallyApproved">Flag indicating if the contract was manually approved.</param>
        /// <param name="fundingType">Flag indicating if the contract funding type.</param>
        /// <param name="principalId">The ID of the current user.</param>
        /// <returns>byte.</returns>
        byte[] AddSignedDocumentPage(byte[] pdf, string contractRefernce, string signer, DateTime signedOn, bool manuallyApproved, ContractFundingType fundingType, string principalId = null);
    }
}
