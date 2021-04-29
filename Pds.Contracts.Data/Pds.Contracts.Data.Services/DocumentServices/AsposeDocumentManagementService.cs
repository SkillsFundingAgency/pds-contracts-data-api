using Aspose.Pdf;
using Aspose.Pdf.Text;
using Pds.Contracts.Data.Services.Extensions;
using Pds.Contracts.Data.Services.Interfaces;
using Pds.Core.Logging;
using System;
using System.Diagnostics;
using System.IO;

namespace Pds.Contracts.Data.Services.DocumentServices
{
    /// <summary>
    /// Aspose implementation of the IDocumentManagementService.
    /// </summary>
    public class AsposeDocumentManagementService : IDocumentManagementService
    {
        private readonly ILoggerAdapter<AsposeDocumentManagementService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsposeDocumentManagementService"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public AsposeDocumentManagementService(ILoggerAdapter<AsposeDocumentManagementService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Create Paragraph.
        /// </summary>
        /// <param name="text">Test.</param>
        /// <returns>TextFragment.</returns>
        public TextFragment CreateParagraph(string text)
        {
            var paragraph = new TextFragment(text)
            {
                Margin = new MarginInfo(10f, 10f, 10f, 10f)
            };

            return paragraph;
        }

        /// <summary>
        /// Create Title.
        /// </summary>
        /// <param name="text">Title text.</param>
        /// <returns>TextFragment.</returns>
        public TextFragment CreateTitle(string text)
        {
            var paragraph = CreateParagraph(text);

            paragraph.TextState.FontStyle = FontStyles.Bold;

            return paragraph;
        }

        /// <summary>
        /// Create text box.
        /// </summary>
        /// <param name="page">Pdf page.</param>
        /// <returns>text box.</returns>
        public FloatingBox CreateTextBox(Page page)
        {
            var floatingBox = new FloatingBox
            {
                Border = new BorderInfo(BorderSide.All, 1, Color.Black),
                Padding = new MarginInfo(10f, 10f, 10f, 10f),
                Margin = new MarginInfo(10f, 100f, 10f, 10f)
            };
            page.Paragraphs.Add(floatingBox);

            return floatingBox;
        }

        /// <summary>
        /// Add image.
        /// </summary>
        /// <param name="page">Pdf page.</param>
        /// <param name="imagePath">Image path.</param>
        /// <param name="overrideWidth">Image width.</param>
        /// <param name="overrideHieght">Imager hieght.</param>
        /// <returns>Image height.</returns>
        public double AddImage(Page page, string imagePath, int? overrideWidth = null, int? overrideHieght = null)
        {
            var widthOffset = 50;
            var heightOffset = 50;

            using (var imageStream = this.GetType().Assembly.GetManifestResourceStream(imagePath))
            {
                var image = System.Drawing.Image.FromStream(imageStream);

                var imageWidth = overrideWidth ?? image.Width / 2;
                var imageHeight = overrideHieght ?? image.Height / 2;
                var pageHeight = page.ArtBox.Height;

                page.AddImage(imageStream, new Rectangle(widthOffset, pageHeight - imageHeight - heightOffset, imageWidth + widthOffset, pageHeight - heightOffset));

                return imageHeight - widthOffset;
            }
        }


        #region IDocumentManagementService Implementation

        /// <summary>
        /// Converts the byte array PDF to be in the PDF\2A_2B format.
        /// </summary>
        /// <param name="pdf">The original PDF.</param>
        /// <returns>The formatted PDF.</returns>
        public byte[] ConvertToPdfA(byte[] pdf)
        {
            _logger.LogInformation("Starting conversion of PDF to PDF_A_2B.");
            var sw = Stopwatch.StartNew();

            using (var inputStream = new MemoryStream(pdf))
            {
                using (var doc = new Document(inputStream))
                {
                    doc.ConvertToPdfA();

                    using (var outputStream = new MemoryStream())
                    {
                        EnsureLandscapePagesAreMarkedCorrectly(doc);
                        doc.Save(outputStream);
                        _logger.LogInformation($"PDF conversion took {sw.ElapsedMilliseconds}ms to complete.");
                        return outputStream.ToArray();
                    }
                }
            }
        }

        #endregion


        #region Addition Helpers

        /// <summary>
        /// EnsureLandscapePagesAreMarkedCorrectly.
        /// </summary>
        /// <param name="document">document.</param>
        protected void EnsureLandscapePagesAreMarkedCorrectly(Document document)
        {
            foreach (Page page in document.Pages)
            {
                if (page.Rect.Width > page.Rect.Height)
                {
                    page.PageInfo.IsLandscape = true;
                }
            }
        }

        /// <summary>
        /// Get memory stream from a byte array.
        /// </summary>
        /// <param name="data">Byte array containing file data.</param>
        /// <returns>A memory stream.</returns>
        private MemoryStream GetMemoryStream(byte[] data)
        {
            var memoryStream = new MemoryStream();
            memoryStream.Write(data, 0, data.Length);

            return memoryStream;
        }

        #endregion
    }
}
