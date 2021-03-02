using Aspose.Pdf;
using Aspose.Pdf.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pds.Contracts.Data.Common.Enums;
using Pds.Contracts.Data.Services.Extensions;
using System;
using System.IO;

namespace Pds.Contracts.Data.Services.Tests.Integration.DocumentServices
{
    public static class ByteExtensions
    {
        /// <summary>
        /// Should have signed page.
        /// </summary>
        /// <param name="pdfAsArray">pdf content as an array.</param>
        /// <param name="contractReference">contract reference number.</param>
        /// <param name="who">who signed contract.</param>
        /// <param name="when">When contract was signed.</param>
        /// <param name="manuallyApproved">has contract been approved manually or electronically.</param>
        /// <param name="fileName">Pdf file name.</param>
        /// <param name="fundingType">Contract funding type.</param>
        /// <param name="principalId">principalId.</param>
        public static void ShouldHaveSignedPage(this byte[] pdfAsArray, string contractReference, string who, DateTime when, bool manuallyApproved, string fileName, ContractFundingType fundingType, string principalId)
        {
            using (var stream = new MemoryStream(pdfAsArray))
            {
                using (var pdf = new Document(stream))
                {
                    var contractType = "contract";

                    if (fundingType == ContractFundingType.Levy)
                    {
                        contractType = "agreement";
                    }

                    pdf.AssertPage1HasText(fileName, $"Signed {contractType} document");

                    if (manuallyApproved)
                    {
                        pdf.AssertPage1HasText(fileName, $"This {contractType} has been signed by the authorised signatory for the Education and " + Environment.NewLine + "Skills Funding Agency, acting on behalf of the Secretary of State.");
                    }
                    else
                    {
                        pdf.AssertPage1HasText(fileName, $"This {contractType} has been signed by the authorised signatory for the Education and " + Environment.NewLine + "Skills Funding Agency, acting on behalf of the Secretary of State, and has been " + Environment.NewLine + "digitally signed by all parties.");
                    }

                    pdf.AssertPage1HasText(fileName, $"Document reference: {contractReference}");
                    pdf.AssertPage1HasText(fileName, $"Signed by {who} on {when.ToDateDisplay()} as the " + Environment.NewLine + "provider's authorised signatory");
                    if (!string.IsNullOrEmpty(principalId))
                    {
                        pdf.AssertPage1HasText(fileName, $"User ID: {principalId}");
                    }

                    // Just in case you want to check the output
                    pdf.Save(fileName);
                }
            }
        }

        private static void AssertPage1HasText(this Document pdf, string fileName, string text)
        {
            var contractReferenceAbsorber = new TextFragmentAbsorber(text);
            pdf.Pages[1].Accept(contractReferenceAbsorber);

            Assert.AreEqual(1, contractReferenceAbsorber.TextFragments.Count, $"Could not find {text} for {fileName}");
        }
    }
}
