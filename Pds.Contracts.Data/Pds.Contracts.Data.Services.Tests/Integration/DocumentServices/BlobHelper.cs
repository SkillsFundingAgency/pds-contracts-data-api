using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Text;

namespace Pds.Contracts.Data.Services.Tests.Integration.DocumentServices
{
    public class BlobHelper
    {
        public static string BlobName => "sample-blob-file.xml";

        public static void CreateSampleBlobFile()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.development.json", optional: false, reloadOnChange: true)
                .Build();
            var sampleBlobFileContent = "<contract xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns='urn:sfa:schemas:contract'></contract>";
            BlobContainerClient containerClient = new BlobContainerClient(configuration.GetSection("AzureBlobConfiguration").GetSection("ConnectionString").Value, "contractevents");
            containerClient.CreateIfNotExists();
            UTF8Encoding encoding = new UTF8Encoding();
            using MemoryStream memoryStream = new MemoryStream(encoding.GetBytes(sampleBlobFileContent));
            containerClient.DeleteBlobIfExists(BlobName);
            containerClient.UploadBlob(BlobName, memoryStream);
        }
    }
}
