using Aspose.Pdf;
using Pds.Contracts.Data.Common.Enums;
using Pds.Contracts.Data.Services.Extensions;
using Pds.Contracts.Data.Services.Interfaces;
using Pds.Core.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Versioning;

namespace Pds.Contracts.Data.Services.DocumentServices
{
    /// <summary>
    /// Aspose Document Management Contract Service.
    /// </summary>
    public class AsposeDocumentManagementContractService : AsposeDocumentManagementService, IDocumentManagementContractService
    {
        private const string OrganisationName = "Department for Education";

        private readonly ILoggerAdapter<AsposeDocumentManagementService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsposeDocumentManagementContractService"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public AsposeDocumentManagementContractService(ILoggerAdapter<AsposeDocumentManagementService> logger) : base(logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Adds a page to the pdf detailing that the contract was digitally signed.
        /// </summary>
        /// <param name="pdf">The PDF to add the page to.</param>
        /// <param name="contractRefernce">The contract reference.</param>
        /// <param name="signer">Who signed the contract.</param>
        /// <param name="signedOn">When the contract was signed.</param>
        /// <param name="manuallyApproved">Flag indicating if the contract was manually approved.</param>
        /// <param name="fundingType">the contract funding type.</param>
        /// <param name="principalId">The ID of the current user.</param>
        /// <returns>The PDF with signed page.</returns>
        [SupportedOSPlatform("windows")]
        public byte[] AddSignedDocumentPage(byte[] pdf, string contractRefernce, string signer, DateTime signedOn, bool manuallyApproved, ContractFundingType fundingType, string principalId = null)
        {
            _logger.LogInformation($"[{nameof(AddSignedDocumentPage)}] Adding signed document page to {contractRefernce}.");

            using (var inputStream = new MemoryStream(pdf))
            {
                using (var doc = new Document(inputStream))
                {
                    //Store width and height of original pages
                    var originalPageInfo = new List<Rectangle>();
                    for (int i = 1; i < doc.Pages.Count + 1; i++)
                    {
                        originalPageInfo.Add(doc.Pages[i].GetPageRect(true));
                    }

                    var newPage = doc.Pages.Insert(1);
                    var cursorLocation = 0d;

                    var imageHeight = AddImage(newPage, $"{AsposeLicenceManagement.EmbededResourcesNamespace}.Logo.png", 170, 105);

                    var top = CreateTextBox(newPage);
                    cursorLocation = imageHeight + 100;
                    top.Top = cursorLocation;

                    var contractType = "contract";

                    if (fundingType == ContractFundingType.Levy)
                    {
                        contractType = "agreement";
                    }

                    top.Paragraphs.Add(CreateTitle($"Signed {contractType} document"));

                    if (manuallyApproved)
                    {
                        top.Paragraphs.Add(CreateParagraph($"This {contractType} has been signed by the authorised signatory for the {OrganisationName}, acting on behalf of the Secretary of State."));
                    }
                    else
                    {
                        top.Paragraphs.Add(CreateParagraph($"This {contractType} has been signed by the authorised signatory for the {OrganisationName}, acting on behalf of the Secretary of State, and has been digitally signed by all parties."));
                    }

                    var bottom = CreateTextBox(newPage);
                    cursorLocation += top.Height + 200;
                    bottom.Top = cursorLocation;
                    bottom.Paragraphs.Add(CreateParagraph($"Document reference: {contractRefernce}"));
                    bottom.Paragraphs.Add(CreateParagraph($"Signed by {signer} on {signedOn.DisplayFormat()} as the provider's authorised signatory"));
                    if (!string.IsNullOrEmpty(principalId))
                    {
                        bottom.Paragraphs.Add(CreateParagraph($"User ID: {principalId}"));
                    }

                    //Set page size from original values
                    for (int i = 0; i < originalPageInfo.Count; i++)
                    {
                        doc.Pages[i + 2].SetPageSize(originalPageInfo[i].Width, originalPageInfo[i].Height);
                    }

                    using (var outputStream = new MemoryStream())
                    {
                        doc.ConvertToPdfA();

                        EnsureLandscapePagesAreMarkedCorrectly(doc);
                        doc.Save(outputStream, SaveFormat.Pdf);
                        _logger.LogInformation($"[{nameof(AddSignedDocumentPage)}] Added signed document page to {contractRefernce}.");
                        return outputStream.ToArray();
                    }
                }
            }
        }
    }
}