using System.Collections.Generic;

namespace Pds.Contracts.Data.Services.Interfaces
{
    /// <summary>
    /// Api for manipulating and getting information from spreadsheets.
    /// </summary>
    public interface IDocumentManagementService
    {
        /// <summary>
        /// Converts the byte array PDF to be in the PDF\2A_2B format.
        /// </summary>
        /// <param name="pdf">The original PDF.</param>
        /// <returns>The formatted PDF.</returns>
        byte[] ConvertToPdfA(byte[] pdf);
    }
}
