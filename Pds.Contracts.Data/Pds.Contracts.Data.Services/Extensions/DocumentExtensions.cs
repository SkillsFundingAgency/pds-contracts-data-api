using Aspose.Pdf;
using System;
using System.IO;

namespace Pds.Contracts.Data.Services.Extensions
{
    /// <summary>
    /// Aspose Pdf document extensions.
    /// </summary>
    public static class DocumentExtensions
    {
        /// <summary>
        /// Convert to PdfA file.
        /// </summary>
        /// <param name="document">The original PDF.</param>
        public static void ConvertToPdfA(this Aspose.Pdf.Document document)
        {
            // At this stage we don't actually want the conversion log file, but we have to supply it
            var logPath = string.Empty;
            var logPath2 = string.Empty;
            try
            {
                logPath2 = Path.GetTempPath();
                logPath = Path.GetTempFileName();

                document.Convert(logPath, PdfFormat.PDF_A_2B, ConvertErrorAction.Delete);

                File.Delete(logPath);
            }
            catch (Exception ex)
            {
                throw new Exception($"PATH : {logPath2} Message {ex.Message} ");
            }
        }
    }
}
