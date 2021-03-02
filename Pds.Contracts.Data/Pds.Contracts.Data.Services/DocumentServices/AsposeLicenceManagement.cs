using Microsoft.Extensions.DependencyInjection;
using System.Resources;

namespace Pds.Contracts.Data.Services.DocumentServices
{
    /// <summary>
    /// Aspose PDF licence management extension.
    /// </summary>
    public static class AsposeLicenceManagement
    {
        /// <summary>
        /// Gets the embeded resources namespace.
        /// </summary>
        /// <value>
        /// The embeded resources namespace.
        /// </value>
        internal static string EmbededResourcesNamespace => "Pds.Contracts.Data.Services.DocumentServices.Resources";

        private static string LicenceFileName => $"{EmbededResourcesNamespace}.Aspose.Total.lic";

        /// <summary>
        /// Adds the aspose license.
        /// </summary>
        /// <param name="serviceCollection">The service collection.</param>
        public static void AddAsposeLicense(this IServiceCollection serviceCollection)
        {
            using var stream = typeof(AsposeLicenceManagement).Assembly.GetManifestResourceStream(LicenceFileName);
            if (stream is null)
            {
                throw new MissingManifestResourceException($"Failed to locate Aspose PDF licence file ({LicenceFileName}) in current assembly.");
            }
            else
            {
                var pdflicense = new Aspose.Pdf.License();
                pdflicense.SetLicense(stream);
            }
        }
    }
}
